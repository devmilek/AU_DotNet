using AU.Domain.Enums;

namespace AU.Domain.Entities;

public class Incident
{
    public const int MaxNameLength = 200;
    public const int MaxCauseLength = 1000;

    public Guid Id { get; private set; }
    public Guid MonitorId { get; private set; }
    public IncidentStatus Status { get; private set; }

    public string? Name { get; private set; }
    public string? Cause { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    public int FailedChecksCount { get; private set; }

    public bool StartedInMaintenance { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Incident() { }

    public bool IsResolved => Status == IncidentStatus.Resolved;

    public static Incident Open(
        Guid monitorId,
        DateTimeOffset startedAt,
        int failedChecksCount,
        bool startedInMaintenance = false,
        string? cause = null)
    {
        if (monitorId == Guid.Empty)
            throw new ArgumentException("MonitorId nie może być pusty.", nameof(monitorId));

        if (failedChecksCount < 1)
            throw new ArgumentOutOfRangeException(nameof(failedChecksCount), "Incydent musi mieć co najmniej 1 nieudany check.");

        var now = DateTimeOffset.UtcNow;

        return new Incident
        {
            Id = Guid.NewGuid(),
            MonitorId = monitorId,
            Status = IncidentStatus.Ongoing,
            StartedAt = startedAt,
            FailedChecksCount = failedChecksCount,
            StartedInMaintenance = startedInMaintenance,
            Cause = Truncate(Normalize(cause), MaxCauseLength),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void IncrementFailedChecks()
    {
        EnsureNotResolved();
        FailedChecksCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Acknowledge(Guid userId, DateTimeOffset acknowledgedAt)
    {
        EnsureNotResolved();

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId nie może być pusty.", nameof(userId));

        if (acknowledgedAt < StartedAt)
            throw new ArgumentException("Data potwierdzenia nie może być wcześniejsza niż started_at.");

        if (AcknowledgedAt is not null)
            return;

        AcknowledgedAt = acknowledgedAt;
        AcknowledgedByUserId = userId;
        Status = IncidentStatus.Acknowledged;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Resolve(DateTimeOffset resolvedAt)
    {
        EnsureNotResolved();

        if (resolvedAt < StartedAt)
            throw new ArgumentException("Data rozwiązania nie może być wcześniejsza niż started_at.");

        ResolvedAt = resolvedAt;
        Status = IncidentStatus.Resolved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDetails(string? name, string? cause)
    {
        name = Normalize(name);
        cause = Normalize(cause);

        if (name?.Length > MaxNameLength)
            throw new ArgumentException($"Nazwa incydentu może mieć maksymalnie {MaxNameLength} znaków.", nameof(name));

        if (cause?.Length > MaxCauseLength)
            throw new ArgumentException($"Przyczyna incydentu może mieć maksymalnie {MaxCauseLength} znaków.", nameof(cause));

        Name = name;
        Cause = cause;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];

    private void EnsureNotResolved()
    {
        if (Status == IncidentStatus.Resolved)
            throw new InvalidOperationException("Nie można modyfikować rozwiązanego incydentu.");
    }
}