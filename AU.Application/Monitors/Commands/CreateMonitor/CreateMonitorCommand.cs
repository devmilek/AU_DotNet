using System.Text.Json.Serialization;
using AU.Domain.Enums;
using AU.Domain.ValueObjects.Checks;

namespace AU.Application.Monitors.Commands.CreateMonitor;

public sealed record CreateMonitorCommand(
    Guid OrganizationId,
    string Name,
    MonitorType Type,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold,
    HttpCheckSettings? Http = null);

/// <summary>Ustawienia HTTP z requestu — sekrety jeszcze jawne, szyfruje je handler.</summary>
public sealed record HttpCheckSettings(
    HttpCheckMethod Method,
    bool FollowRedirects,
    IReadOnlyList<StatusCodeRangeSettings>? AcceptedStatusCodes,
    HttpAuthSettings? Auth);

public sealed record StatusCodeRangeSettings(int From, int To);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HttpAuthType
{
    None,
    Basic,
    Bearer
}

public sealed record HttpAuthSettings(
    HttpAuthType Type,
    string? Username,
    string? Password,
    string? Token);
