using US.Domain.Enums;

namespace US.Api.Controllers.Monitors.Requests;

public sealed record CreateMonitorRequest(
    string Name,
    MonitorType Type,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold);