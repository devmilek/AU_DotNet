using AU.Application.Monitors.Commands.CreateMonitor;

namespace AU.Api.Controllers.Monitors.Requests;

public sealed record UpdateMonitorRequest(
    string Name,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold,
    HttpCheckSettings? Http = null);
