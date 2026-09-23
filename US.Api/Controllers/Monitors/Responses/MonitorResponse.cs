using US.Application.Monitors.Commands.CreateMonitor;
using US.Domain.Enums;
using US.Domain.ValueObjects.Checks;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Api.Controllers.Monitors.Responses;

/// <summary>
/// Konfiguracja monitora zwracana przez API. Sekrety (hasła, tokeny) nigdy nie są zwracane —
/// tylko informacja, czy są ustawione.
/// </summary>
public sealed record MonitorResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    MonitorType Type,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold,
    bool NotifyOnRecovery,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    HttpCheckConfigResponse? Http)
{
    public static MonitorResponse From(Monitor monitor) => new(
        monitor.Id,
        monitor.OrganizationId,
        monitor.Name,
        monitor.Type,
        monitor.Target,
        monitor.IntervalSeconds,
        monitor.TimeoutMs,
        monitor.AlertThreshold,
        monitor.RecoveryThreshold,
        monitor.NotifyOnRecovery,
        monitor.IsActive,
        monitor.CreatedAt,
        monitor.UpdatedAt,
        monitor.Config is HttpCheckConfig http ? HttpCheckConfigResponse.From(http) : null);
}

public sealed record HttpCheckConfigResponse(
    HttpCheckMethod Method,
    bool FollowRedirects,
    IReadOnlyList<StatusCodeRangeSettings> AcceptedStatusCodes,
    HttpAuthResponse Auth)
{
    public static HttpCheckConfigResponse From(HttpCheckConfig config) => new(
        config.Method,
        config.FollowRedirects,
        config.AcceptedStatusCodes.Select(r => new StatusCodeRangeSettings(r.From, r.To)).ToList(),
        config.Auth switch
        {
            BasicHttpAuth basic => new HttpAuthResponse(HttpAuthType.Basic, basic.Username, HasPassword: true, HasToken: false),
            BearerHttpAuth => new HttpAuthResponse(HttpAuthType.Bearer, null, HasPassword: false, HasToken: true),
            _ => new HttpAuthResponse(HttpAuthType.None, null, HasPassword: false, HasToken: false)
        });
}

public sealed record HttpAuthResponse(
    HttpAuthType Type,
    string? Username,
    bool HasPassword,
    bool HasToken);
