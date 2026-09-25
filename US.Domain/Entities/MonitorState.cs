namespace US.Domain.Entities;

public class MonitorState
{
    public Guid MonitorId { get; private set; }

    public int ConsecutiveSuccesses { get; private set; }
    public int ConsecutiveFailures { get; private set; }

    public DateTimeOffset NextCheckAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private MonitorState() { }

    internal static MonitorState Create(Guid monitorId, DateTimeOffset now)
    {
        return new MonitorState
        {
            MonitorId = monitorId,
            ConsecutiveSuccesses = 0,
            ConsecutiveFailures = 0,
            NextCheckAt = now,
            UpdatedAt = now
        };
    }

    internal void ScheduleNextCheck(DateTimeOffset now, int intervalSeconds)
    {
        NextCheckAt = now.AddSeconds(intervalSeconds);
        Touch();
    }

    internal void RecordFailure()
    {
        ConsecutiveSuccesses = 0;
        ConsecutiveFailures++;
        Touch();
    }

    internal void RecordSuccess()
    {
        ConsecutiveFailures = 0;
        ConsecutiveSuccesses++;
        Touch();
    }

    internal void ResetCounters()
    {
        ConsecutiveFailures = 0;
        ConsecutiveSuccesses = 0;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
