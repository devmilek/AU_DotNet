namespace US.Domain.Entities;

public enum MaintenanceOccurrenceStatus
{
    Scheduled,
    Cancelled
}

public class MaintenanceOccurrence
{
    public Guid Id { get; private set; }

    public Guid? MaintenanceWindowId { get; private set; }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public DateTimeOffset? ContentLockedAt { get; private set; }

    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }

    public DateTimeOffset ScheduledStartUtc { get; private set; }

    public MaintenanceOccurrenceStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsCancelled => Status == MaintenanceOccurrenceStatus.Cancelled;
    public bool IsRescheduled => StartsAtUtc != ScheduledStartUtc;
    public bool IsContentLocked => ContentLockedAt is not null;

    private MaintenanceOccurrence() { }

    internal static MaintenanceOccurrence Create(
        Guid maintenanceWindowId,
        DateTimeOffset scheduledStartUtc,
        TimeSpan duration,
        string name,
        string? description,
        DateTimeOffset now)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Czas trwania musi być dodatni.");

        var start = scheduledStartUtc.ToUniversalTime();

        return new MaintenanceOccurrence
        {
            Id = Guid.CreateVersion7(),
            MaintenanceWindowId = maintenanceWindowId,
            Name = name,
            Description = description,
            ScheduledStartUtc = start,
            StartsAtUtc = start,
            EndsAtUtc = start + duration,
            Status = MaintenanceOccurrenceStatus.Scheduled,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public bool IsActiveAt(DateTimeOffset instant) =>
        !IsCancelled && StartsAtUtc <= instant && instant < EndsAtUtc;

    public bool HasStarted(DateTimeOffset now) => StartsAtUtc <= now;

    public bool HasEnded(DateTimeOffset now) => EndsAtUtc <= now;

    public void LockContent(DateTimeOffset lockedAt)
    {
        ContentLockedAt ??= lockedAt;
    }

    public void OverrideContent(string name, string? description, DateTimeOffset now)
    {
        MaintenanceWindow.ValidateName(name);

        Name = name.Trim();
        Description = MaintenanceWindow.NormalizeDescription(description);
        LockContent(now);
        Touch(now);
    }

    internal void ApplyContent(string name, string? description, DateTimeOffset now)
    {
        if (Name == name && Description == description) return;

        Name = name;
        Description = description;
        Touch(now);
    }

    internal void ApplyDuration(TimeSpan duration, DateTimeOffset now)
    {
        if (HasStarted(now) || IsRescheduled) return;

        var endsAt = StartsAtUtc + duration;
        if (endsAt == EndsAtUtc) return;

        EndsAtUtc = endsAt;
        Touch(now);
    }

    public void Reschedule(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, DateTimeOffset now)
    {
        EnsureNotCancelled();

        if (HasStarted(now))
            throw new InvalidOperationException("Nie można przesunąć wystąpienia, które już się rozpoczęło.");

        if (endsAtUtc <= startsAtUtc)
            throw new ArgumentException("Koniec wystąpienia musi być późniejszy niż początek.", nameof(endsAtUtc));

        if (endsAtUtc <= now)
            throw new ArgumentException("Nie można przesunąć wystąpienia w przeszłość.", nameof(endsAtUtc));

        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
        Touch(now);
    }

    public void Extend(DateTimeOffset endsAtUtc, DateTimeOffset now)
    {
        EnsureNotCancelled();

        if (HasEnded(now))
            throw new InvalidOperationException("Nie można wydłużyć zakończonego wystąpienia.");

        if (endsAtUtc <= EndsAtUtc)
            throw new ArgumentException("Nowy koniec musi być późniejszy niż obecny.", nameof(endsAtUtc));

        EndsAtUtc = endsAtUtc.ToUniversalTime();
        Touch(now);
    }

    public void EndEarly(DateTimeOffset now)
    {
        EnsureNotCancelled();

        if (!IsActiveAt(now))
            throw new InvalidOperationException("Wcześniej zakończyć można tylko trwające wystąpienie.");

        EndsAtUtc = now.ToUniversalTime();
        LockContent(StartsAtUtc);
        Touch(now);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (IsCancelled) return;

        if (HasStarted(now))
            throw new InvalidOperationException("Nie można anulować wystąpienia, które już się rozpoczęło.");

        Status = MaintenanceOccurrenceStatus.Cancelled;
        Touch(now);
    }

    private void EnsureNotCancelled()
    {
        if (IsCancelled)
            throw new InvalidOperationException("Nie można modyfikować anulowanego wystąpienia.");
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;
}
