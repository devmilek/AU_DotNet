using US.Domain.Enums;

namespace US.Application.Monitors.Commands.CreateMonitor;

public sealed record CreateMonitorCommand(
    Guid OrganizationId,
    string Name,
    MonitorType Type,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold);