namespace US.Domain.Entities;

public enum MaintenanceOccurrenceStatus
{
    Scheduled,
    Cancelled
}

public class MaintenanceOccurrence
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Null po twardym usunięciu okna (ON DELETE SET NULL) — historia checków zostaje.
    /// </summary>
    public Guid? MaintenanceWindowId { get; private set; }

    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }

    /// <summary>
    /// Pierwotny termin wyznaczony z reguły — klucz do deduplikacji przy generowaniu
    /// wystąpień, nie zmienia się przy przesunięciu pojedynczego wystąpienia.
    /// </summary>
    public DateTimeOffset ScheduledStartUtc { get; private set; }

    public MaintenanceOccurrenceStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsCancelled => Status == MaintenanceOccurrenceStatus.Cancelled;
    public bool IsRescheduled => StartsAtUtc != ScheduledStartUtc;

    private MaintenanceOccurrence() { }

    internal static MaintenanceOccurrence Create(Guid maintenanceWindowId, DateTimeOffset scheduledStartUtc, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Czas trwania musi być dodatni.");

        var start = scheduledStartUtc.ToUniversalTime();
        var now = DateTimeOffset.UtcNow;

        return new MaintenanceOccurrence
        {
            Id = Guid.CreateVersion7(),
            MaintenanceWindowId = maintenanceWindowId,
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

    /// <summary>
    /// Przesuwa tylko to wystąpienie (wyjątek od reguły). Nie dotyczy trwających ani zakończonych.
    /// </summary>
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

    /// <summary>
    /// Wydłuża trwające lub przyszłe wystąpienie.
    /// </summary>
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

    /// <summary>
    /// Kończy trwające wystąpienie teraz. Checki sprzed tej chwili zostają oznaczone jako maintenance.
    /// </summary>
    public void EndEarly(DateTimeOffset now)
    {
        EnsureNotCancelled();

        if (!IsActiveAt(now))
            throw new InvalidOperationException("Wcześniej zakończyć można tylko trwające wystąpienie.");

        EndsAtUtc = now.ToUniversalTime();
        Touch(now);
    }

    /// <summary>
    /// Anuluje przyszłe wystąpienie. Trwające kończ przez <see cref="EndEarly"/>,
    /// żeby nie przekłamać historii checków oznaczonych jako maintenance.
    /// </summary>
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
