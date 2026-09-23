using US.Application.Monitors.Commands.CreateMonitor;
using US.Domain.Enums;

namespace US.Api.Controllers.Monitors.Requests;

public sealed record CreateMonitorRequest(
    string Name,
    MonitorType Type,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold,
    /// <summary>Ustawienia HTTP — tylko dla monitora typu Http. Pominięte = domyślne (GET, follow redirects, 200-299).</summary>
    HttpCheckSettings? Http = null);