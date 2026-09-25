namespace US.Domain.Entities;

public class MaintenanceWindow
{
    private const int MinDurationMinutes = 1;
    private const int MaxDurationMinutes = 30 * 24 * 60;

    private readonly List<MaintenanceWindowMonitor> _monitors = [];

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    /// <summary>
    /// Identyfikator strefy IANA, np. "Europe/Warsaw".
    /// </summary>
    public string TimeZoneId { get; private set; } = null!;

    /// <summary>
    /// Czas ścienny (w <see cref="TimeZoneId"/>) pierwszego wystąpienia.
    /// </summary>
    public DateTime StartsAtLocal { get; private set; }

    public int DurationMinutes { get; private set; }

    /// <summary>
    /// RRULE (RFC 5545) bez prefiksu "RRULE:" i bez DTSTART. Null = okno jednorazowe.
    /// </summary>
    public string? RecurrenceRule { get; private set; }

    /// <summary>
    /// Opcjonalny koniec serii (czas ścienny) — ostatnie wystąpienie nie może zacząć się później.
    /// </summary>
    public DateTime? RecurrenceEndLocal { get; private set; }

    public bool SuppressNotifications { get; private set; }
    public bool ExcludeFromSla { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<MaintenanceWindowMonitor> Monitors => _monitors.AsReadOnly();

    public bool IsRecurring => RecurrenceRule is not null;
    public bool IsDeleted => DeletedAt is not null;
    public TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);

    private MaintenanceWindow() { }

    public static MaintenanceWindow Create(
        Guid organizationId,
        Guid createdByUserId,
        string name,
        string? description,
        string timeZoneId,
        DateTime startsAtLocal,
        int durationMinutes,
        IEnumerable<Guid> monitorIds,
        string? recurrenceRule = null,
        DateTime? recurrenceEndLocal = null,
        bool suppressNotifications = true,
        bool excludeFromSla = true)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("OrganizationId nie może być pusty.", nameof(organizationId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId nie może być pusty.", nameof(createdByUserId));

        ValidateName(name);
        ValidateTimeZone(timeZoneId);
        ValidateLocal(startsAtLocal, nameof(startsAtLocal));
        ValidateDuration(durationMinutes);
        var rule = NormalizeRecurrenceRule(recurrenceRule);
        ValidateRecurrenceEnd(rule, startsAtLocal, recurrenceEndLocal);

        var now = DateTimeOffset.UtcNow;

        var window = new MaintenanceWindow
        {
            Id = Guid.CreateVersion7(),
            OrganizationId = organizationId,
            CreatedByUserId = createdByUserId,
            Name = name.Trim(),
            Description = NormalizeDescription(description),
            TimeZoneId = timeZoneId,
            StartsAtLocal = startsAtLocal,
            DurationMinutes = durationMinutes,
            RecurrenceRule = rule,
            RecurrenceEndLocal = recurrenceEndLocal,
            SuppressNotifications = suppressNotifications,
            ExcludeFromSla = excludeFromSla,
            CreatedAt = now,
            UpdatedAt = now
        };

        window.ReplaceMonitors(monitorIds);

        return window;
    }

    public void UpdateDetails(string name, string? description)
    {
        EnsureNotDeleted();
        ValidateName(name);

        Name = name.Trim();
        Description = NormalizeDescription(description);
        Touch();
    }

    /// <summary>
    /// Zmienia harmonogram. Wywołujący odpowiada za przegenerowanie przyszłych
    /// <see cref="MaintenanceOccurrence"/> (przeszłe i trwające zostają bez zmian).
    /// </summary>
    public void UpdateSchedule(
        string timeZoneId,
        DateTime startsAtLocal,
        int durationMinutes,
        string? recurrenceRule,
        DateTime? recurrenceEndLocal)
    {
        EnsureNotDeleted();
        ValidateTimeZone(timeZoneId);
        ValidateLocal(startsAtLocal, nameof(startsAtLocal));
        ValidateDuration(durationMinutes);
        var rule = NormalizeRecurrenceRule(recurrenceRule);
        ValidateRecurrenceEnd(rule, startsAtLocal, recurrenceEndLocal);

        TimeZoneId = timeZoneId;
        StartsAtLocal = startsAtLocal;
        DurationMinutes = durationMinutes;
        RecurrenceRule = rule;
        RecurrenceEndLocal = recurrenceEndLocal;
        Touch();
    }

    public void UpdatePolicy(bool suppressNotifications, bool excludeFromSla)
    {
        EnsureNotDeleted();

        SuppressNotifications = suppressNotifications;
        ExcludeFromSla = excludeFromSla;
        Touch();
    }

    /// <summary>
    /// Podmienia zestaw monitorów objętych oknem. Przynależność monitorów do organizacji
    /// musi zweryfikować warstwa aplikacji.
    /// </summary>
    public void SetMonitors(IEnumerable<Guid> monitorIds)
    {
        EnsureNotDeleted();
        ReplaceMonitors(monitorIds);
        Touch();
    }

    public bool Covers(Guid monitorId) => _monitors.Any(m => m.MonitorId == monitorId);

    /// <summary>
    /// Soft delete. Wywołujący powinien anulować przyszłe wystąpienia
    /// (<see cref="MaintenanceOccurrence.Cancel"/>) i zakończyć trwające (<see cref="MaintenanceOccurrence.EndEarly"/>).
    /// </summary>
    public void Delete(DateTimeOffset now)
    {
        if (IsDeleted) return;

        DeletedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Tworzy wystąpienie dla terminu wyznaczonego z reguły (czas ścienny w strefie okna).
    /// Rozwinięcie RRULE na listę terminów odbywa się poza domeną.
    /// </summary>
    public MaintenanceOccurrence CreateOccurrence(DateTime scheduledStartLocal)
    {
        EnsureNotDeleted();
        ValidateLocal(scheduledStartLocal, nameof(scheduledStartLocal));

        if (!IsRecurring && scheduledStartLocal != StartsAtLocal)
            throw new ArgumentException(
                "Okno jednorazowe ma tylko jedno wystąpienie — w StartsAtLocal.", nameof(scheduledStartLocal));

        if (scheduledStartLocal < StartsAtLocal)
            throw new ArgumentException(
                "Wystąpienie nie może zaczynać się przed pierwszym terminem okna.", nameof(scheduledStartLocal));

        if (RecurrenceEndLocal is { } end && scheduledStartLocal > end)
            throw new ArgumentException(
                "Wystąpienie nie może zaczynać się po końcu serii.", nameof(scheduledStartLocal));

        var scheduledStartUtc = ToUtc(scheduledStartLocal);

        return MaintenanceOccurrence.Create(Id, scheduledStartUtc, Duration);
    }

    /// <summary>
    /// Zamienia czas ścienny w strefie okna na UTC zgodnie z RFC 5545:
    /// czas nieistniejący (przejście na czas letni) liczony jest offsetem sprzed zmiany,
    /// czas niejednoznaczny (powrót na czas zimowy) — jako pierwsze wystąpienie.
    /// </summary>
    public DateTimeOffset ToUtc(DateTime local)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);

        TimeSpan offset;

        if (tz.IsInvalidTime(local))
        {
            var probe = local;
            while (tz.IsInvalidTime(probe))
                probe = probe.AddMinutes(-15);

            offset = tz.GetUtcOffset(probe);
        }
        else if (tz.IsAmbiguousTime(local))
        {
            offset = tz.GetAmbiguousTimeOffsets(local).Max();
        }
        else
        {
            offset = tz.GetUtcOffset(local);
        }

        return new DateTimeOffset(local.Ticks - offset.Ticks, TimeSpan.Zero);
    }

    private void ReplaceMonitors(IEnumerable<Guid> monitorIds)
    {
        var ids = monitorIds.Distinct().ToList();

        if (ids.Count == 0)
            throw new ArgumentException("Okno serwisowe musi obejmować co najmniej jeden monitor.", nameof(monitorIds));

        if (ids.Contains(Guid.Empty))
            throw new ArgumentException("MonitorId nie może być pusty.", nameof(monitorIds));

        _monitors.RemoveAll(m => !ids.Contains(m.MonitorId));

        foreach (var id in ids.Where(id => !Covers(id)))
            _monitors.Add(new MaintenanceWindowMonitor(id, Id));
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Nie można modyfikować usuniętego okna serwisowego.");
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa okna serwisowego nie może być pusta.", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Nazwa okna serwisowego nie może przekraczać 200 znaków.", nameof(name));
    }

    private static void ValidateTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)
            || !TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz)
            || !tz.HasIanaId)
            throw new ArgumentException($"Nieznana strefa czasowa IANA: '{timeZoneId}'.", nameof(timeZoneId));
    }

    private static void ValidateLocal(DateTime value, string paramName)
    {
        // Utc/Local oznaczałoby, że ktoś już przeliczył czas — a tu chcemy czystego czasu ściennego
        if (value.Kind != DateTimeKind.Unspecified)
            throw new ArgumentException("Czas ścienny musi mieć DateTimeKind.Unspecified.", paramName);
    }

    private static void ValidateDuration(int durationMinutes)
    {
        if (durationMinutes is < MinDurationMinutes or > MaxDurationMinutes)
            throw new ArgumentOutOfRangeException(
                nameof(durationMinutes),
                $"Czas trwania musi mieścić się w zakresie {MinDurationMinutes}-{MaxDurationMinutes} minut.");
    }

    private static string? NormalizeRecurrenceRule(string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
            return null;

        rule = rule.Trim().ToUpperInvariant();

        if (rule.StartsWith("RRULE:"))
            rule = rule["RRULE:".Length..];

        var parts = rule.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (!parts.Any(p => p.StartsWith("FREQ=")))
            throw new ArgumentException("RRULE musi zawierać FREQ.", nameof(rule));

        if (rule.Contains("DTSTART"))
            throw new ArgumentException("RRULE nie może zawierać DTSTART — początek to StartsAtLocal.", nameof(rule));

        return string.Join(';', parts);
    }

    private static void ValidateRecurrenceEnd(string? rule, DateTime startsAtLocal, DateTime? recurrenceEndLocal)
    {
        if (recurrenceEndLocal is not { } end)
            return;

        if (rule is null)
            throw new ArgumentException("Koniec serii ma sens tylko dla okna cyklicznego.", nameof(recurrenceEndLocal));

        ValidateLocal(end, nameof(recurrenceEndLocal));

        if (end < startsAtLocal)
            throw new ArgumentException("Koniec serii nie może być wcześniejszy niż pierwsze wystąpienie.", nameof(recurrenceEndLocal));

        if (rule.Contains("UNTIL=") || rule.Contains("COUNT="))
            throw new ArgumentException(
                "Podaj koniec serii albo UNTIL/COUNT w RRULE, nie oba naraz.", nameof(recurrenceEndLocal));
    }
}
