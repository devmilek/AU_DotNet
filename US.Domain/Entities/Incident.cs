using US.Domain.Enums;

namespace US.Domain.Entities;

public class Incident
{
    public Guid Id { get; private set; }
    public Guid MonitorId { get; private set; }
    public IncidentStatus Status { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    public int FailedChecksCount { get; private set; }

    public bool StartedInMaintenance { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Incident() { }

    public static Incident Open(Guid monitorId, DateTimeOffset startedAt, int failedChecksCount, bool startedInMaintenance = false)
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

    public void Acknowledge(DateTimeOffset acknowledgedAt)
    {
        EnsureNotResolved();

        if (acknowledgedAt < StartedAt)
            throw new ArgumentException("Data potwierdzenia nie może być wcześniejsza niż started_at.");

        AcknowledgedAt = acknowledgedAt;
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

    private void EnsureNotResolved()
    {
        if (Status == IncidentStatus.Resolved)
            throw new InvalidOperationException("Nie można modyfikować rozwiązanego incydentu.");
    }
}