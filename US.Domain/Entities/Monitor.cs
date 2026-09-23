using System.Text.Json.Serialization;
using US.Domain.Enums;

namespace US.Domain.Entities;

public class Monitor
{
    private const int MinIntervalSeconds = 5;
    private const int MinTimeoutMs = 100;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Target { get; private set; } = null!;

    public MonitorType Type { get; private set; }
    public int IntervalSeconds { get; private set; }
    public int TimeoutMs { get; private set; }

    public int AlertThreshold { get; private set; }
    public int RecoveryThreshold { get; private set; }

    public bool NotifyOnRecovery { get; private set; }

    public bool IsActive { get; private set; }
    
    public Guid OrganizationId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // stan runtime jest wewnętrzny — nie wystawiamy go w API
    [JsonIgnore]
    public MonitorState State { get; private set; } = null!;

    private Monitor() { }

    public static Monitor Create(
        Guid organizationId,
        string name,
        string target,
        MonitorType type,
        int intervalSeconds = 60,
        int timeoutMs = 5000,
        int alertThreshold = 3,
        int recoveryThreshold = 3,
        bool notifyOnRecovery = true)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "OrganizationId nie może być pusty.",
                nameof(organizationId));

        ValidateName(name);
        ValidateTarget(target);
        ValidateInterval(intervalSeconds);
        ValidateTimeout(timeoutMs, intervalSeconds);
        ValidateThreshold(alertThreshold, nameof(alertThreshold));
        ValidateThreshold(recoveryThreshold, nameof(recoveryThreshold));

        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid();

        return new Monitor
        {
            Id = id,
            OrganizationId = organizationId,
            Name = name.Trim(),
            Target = target.Trim(),
            Type = type,
            IntervalSeconds = intervalSeconds,
            TimeoutMs = timeoutMs,
            AlertThreshold = alertThreshold,
            RecoveryThreshold = recoveryThreshold,
            NotifyOnRecovery = notifyOnRecovery,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            State = MonitorState.Create(id, now)
        };
    }
    
    public void ScheduleNextCheck(DateTimeOffset now)
    {
        State.ScheduleNextCheck(now, IntervalSeconds);
    }

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name.Trim();
        Touch();
    }

    public void UpdateTarget(string target)
    {
        ValidateTarget(target);
        Target = target.Trim();
        Touch();
    }

    public void UpdateSchedule(int intervalSeconds, int timeoutMs)
    {
        ValidateInterval(intervalSeconds);
        ValidateTimeout(timeoutMs, intervalSeconds);

        IntervalSeconds = intervalSeconds;
        TimeoutMs = timeoutMs;
        Touch();
    }

    public void UpdateThresholds(int alertThreshold, int recoveryThreshold)
    {
        ValidateThreshold(alertThreshold, nameof(alertThreshold));
        ValidateThreshold(recoveryThreshold, nameof(recoveryThreshold));

        AlertThreshold = alertThreshold;
        RecoveryThreshold = recoveryThreshold;
        Touch();
    }

    public void SetNotifyOnRecovery(bool notifyOnRecovery)
    {
        NotifyOnRecovery = notifyOnRecovery;
        Touch();
    }

    /// <summary>
    /// Rejestruje nieudany check. Zwraca true, jeśli licznik osiągnął alert_threshold
    /// (czyli powinien zostać otwarty nowy incydent).
    /// </summary>
    public bool RecordFailure()
    {
        if (!IsActive)
            throw new InvalidOperationException("Nie można rejestrować checków dla nieaktywnego monitora.");

        State.RecordFailure();

        return State.ConsecutiveFailures >= AlertThreshold;
    }

    /// <summary>
    /// Rejestruje udany check i resetuje licznik. Zwraca true, jeśli monitor
    /// wychodził właśnie z serii błędów (czyli można rozważyć zamknięcie incydentu).
    /// </summary>
    public bool RecordSuccess()
    {
        if (!IsActive)
            throw new InvalidOperationException("Nie można rejestrować checków dla nieaktywnego monitora.");

        State.RecordSuccess();

        return State.ConsecutiveSuccesses == RecoveryThreshold;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        State.ResetCounters();
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa monitora nie może być pusta.", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Nazwa monitora nie może przekraczać 200 znaków.", nameof(name));
    }

    private static void ValidateTarget(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException("Target monitora nie może być pusty.", nameof(target));
    }

    private static void ValidateInterval(int intervalSeconds)
    {
        if (intervalSeconds < MinIntervalSeconds)
            throw new ArgumentOutOfRangeException(
                nameof(intervalSeconds),
                $"Interval musi wynosić co najmniej {MinIntervalSeconds}s.");
    }

    private static void ValidateTimeout(int timeoutMs, int intervalSeconds)
    {
        if (timeoutMs < MinTimeoutMs)
            throw new ArgumentOutOfRangeException(
                nameof(timeoutMs),
                $"Timeout musi wynosić co najmniej {MinTimeoutMs}ms.");

        if (timeoutMs >= intervalSeconds * 1000)
            throw new ArgumentException(
                "Timeout musi być mniejszy niż interval (w ms), inaczej checki będą się nakładać.");
    }

    private static void ValidateThreshold(int threshold, string paramName)
    {
        if (threshold < 1)
            throw new ArgumentOutOfRangeException(paramName, "Threshold musi być większy od zera.");
    }
}
