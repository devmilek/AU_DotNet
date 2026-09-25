namespace US.Domain.Entities;

public class MaintenanceWindow
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxRecurrenceRuleLength = 500;
    public const int MinDurationMinutes = 1;
    public const int MaxDurationMinutes = 30 * 24 * 60;

    private readonly List<MaintenanceWindowMonitor> _monitors = [];
    private readonly List<MaintenanceOccurrence> _occurrences = [];

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public string TimeZoneId { get; private set; } = null!;

    public DateTime StartsAtLocal { get; private set; }

    public int DurationMinutes { get; private set; }

    public string? RecurrenceRule { get; private set; }

    public DateTime? RecurrenceEndLocal { get; private set; }

    public bool SuppressNotifications { get; private set; }
    public bool ExcludeFromSla { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<MaintenanceWindowMonitor> Monitors => _monitors.AsReadOnly();

    public IReadOnlyCollection<MaintenanceOccurrence> Occurrences => _occurrences.AsReadOnly();

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
        string? recurrenceRule,
        DateTime? recurrenceEndLocal,
        bool suppressNotifications,
        bool excludeFromSla,
        DateTimeOffset now)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("OrganizationId nie może być pusty.", nameof(organizationId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId nie może być pusty.", nameof(createdByUserId));

        ValidateName(name);
        ValidateDescription(description);
        ValidateTimeZone(timeZoneId);
        ValidateLocal(startsAtLocal, nameof(startsAtLocal));
        ValidateDuration(durationMinutes);
        var rule = NormalizeRecurrenceRule(recurrenceRule);
        ValidateRecurrenceEnd(rule, startsAtLocal, recurrenceEndLocal);

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

        if (window._monitors.Count == 0)
            throw new ArgumentException("Okno serwisowe musi obejmować co najmniej jeden monitor.", nameof(monitorIds));

        return window;
    }

    public void Rename(string name, string? description, bool includePastOccurrences, DateTimeOffset now)
    {
        EnsureNotDeleted();
        ValidateName(name);
        ValidateDescription(description);

        Name = name.Trim();
        Description = NormalizeDescription(description);

        foreach (var occurrence in _occurrences)
        {
            if (occurrence.HasStarted(now))
            {
                occurrence.LockContent(occurrence.StartsAtUtc);

                if (includePastOccurrences)
                    occurrence.ApplyContent(Name, Description, now);
            }
            else if (!occurrence.IsContentLocked)
            {
                occurrence.ApplyContent(Name, Description, now);
            }
        }

        UpdatedAt = now;
    }

    public void UpdateSchedule(
        string timeZoneId,
        DateTime startsAtLocal,
        int durationMinutes,
        string? recurrenceRule,
        DateTime? recurrenceEndLocal,
        DateTimeOffset now)
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
        UpdatedAt = now;
    }

    public void UpdatePolicy(bool suppressNotifications, bool excludeFromSla, DateTimeOffset now)
    {
        EnsureNotDeleted();

        SuppressNotifications = suppressNotifications;
        ExcludeFromSla = excludeFromSla;
        UpdatedAt = now;
    }

    public void SetMonitors(IEnumerable<Guid> monitorIds, DateTimeOffset now)
    {
        EnsureNotDeleted();
        ReplaceMonitors(monitorIds);
        UpdatedAt = now;
    }

    public bool AssignMonitor(Guid monitorId, DateTimeOffset now)
    {
        EnsureNotDeleted();

        if (monitorId == Guid.Empty)
            throw new ArgumentException("MonitorId nie może być pusty.", nameof(monitorId));

        if (Covers(monitorId)) return false;

        _monitors.Add(new MaintenanceWindowMonitor(monitorId, Id));
        UpdatedAt = now;
        return true;
    }

    public bool UnassignMonitor(Guid monitorId, DateTimeOffset now)
    {
        EnsureNotDeleted();

        if (_monitors.RemoveAll(m => m.MonitorId == monitorId) == 0) return false;

        UpdatedAt = now;
        return true;
    }

    public bool Covers(Guid monitorId) => _monitors.Any(m => m.MonitorId == monitorId);

    public IReadOnlyList<MaintenanceOccurrence> SyncOccurrences(
        IEnumerable<DateTime> scheduledStartsLocal,
        DateTimeOffset now)
    {
        EnsureNotDeleted();

        var planned = scheduledStartsLocal
            .Select(local =>
            {
                ValidateScheduledStart(local);
                return ToUtc(local);
            })
            .Distinct()
            .Order()
            .ToList();

        for (var i = 1; i < planned.Count; i++)
        {
            if (planned[i - 1] + Duration > planned[i])
                throw new ArgumentException(
                    "Wystąpienia okna nakładają się — czas trwania jest dłuższy niż odstęp między nimi.",
                    nameof(scheduledStartsLocal));
        }

        var plannedSet = planned.ToHashSet();

        var removed = _occurrences
            .Where(o => !o.HasStarted(now) && !plannedSet.Contains(o.ScheduledStartUtc))
            .ToList();

        foreach (var occurrence in removed)
            _occurrences.Remove(occurrence);

        var existing = _occurrences.ToDictionary(o => o.ScheduledStartUtc);

        foreach (var start in planned)
        {
            if (existing.TryGetValue(start, out var occurrence))
            {
                occurrence.ApplyDuration(Duration, now);
                continue;
            }

            if (start + Duration <= now) continue;

            _occurrences.Add(MaintenanceOccurrence.Create(Id, start, Duration, Name, Description, now));
        }

        return removed;
    }

    public void Delete(DateTimeOffset now)
    {
        if (IsDeleted) return;

        _occurrences.RemoveAll(o => !o.HasStarted(now));

        foreach (var occurrence in _occurrences.Where(o => o.IsActiveAt(now)))
            occurrence.EndEarly(now);

        DeletedAt = now;
        UpdatedAt = now;
    }

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

    public static bool IsKnownTimeZone(string? timeZoneId) =>
        !string.IsNullOrWhiteSpace(timeZoneId)
        && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz)
        && tz.HasIanaId;

    private void ValidateScheduledStart(DateTime scheduledStartLocal)
    {
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
    }

    private void ReplaceMonitors(IEnumerable<Guid> monitorIds)
    {
        var ids = monitorIds.ToHashSet();

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

    internal static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    internal static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa okna serwisowego nie może być pusta.", nameof(name));

        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException($"Nazwa okna serwisowego nie może przekraczać {MaxNameLength} znaków.", nameof(name));
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Trim().Length > MaxDescriptionLength)
            throw new ArgumentException($"Opis nie może przekraczać {MaxDescriptionLength} znaków.", nameof(description));
    }

    private static void ValidateTimeZone(string timeZoneId)
    {
        if (!IsKnownTimeZone(timeZoneId))
            throw new ArgumentException($"Nieznana strefa czasowa IANA: '{timeZoneId}'.", nameof(timeZoneId));
    }

    private static void ValidateLocal(DateTime value, string paramName)
    {
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

        rule = string.Join(';', parts);

        if (rule.Length > MaxRecurrenceRuleLength)
            throw new ArgumentException($"RRULE nie może przekraczać {MaxRecurrenceRuleLength} znaków.", nameof(rule));

        return rule;
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
