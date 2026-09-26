using AU.Application.Channels;
using AU.Application.Channels.Queries.GetNotificationChannel;
using AU.Domain.Entities;
using AU.Domain.Enums;
using AU.Domain.ValueObjects;

namespace AU.Api.Controllers.Channels.Responses;

/// <summary>
/// Konfiguracja per typ; sekrety (np. przyszły secret webhooka) nigdy nie są zwracane.
/// Tylko jedno z pól jest wypełnione — zgodne z <see cref="ChannelType"/>.
/// </summary>
public sealed record ChannelConfigResponse(EmailChannelConfigResponse? Email)
{
    public static ChannelConfigResponse From(ChannelConfig config) => config switch
    {
        EmailChannelConfig email => new ChannelConfigResponse(new EmailChannelConfigResponse(email.To)),
        _ => new ChannelConfigResponse(Email: null)
    };
}

public sealed record EmailChannelConfigResponse(IReadOnlyList<string> To);

public sealed record NotificationChannelListItemResponse(
    Guid Id,
    string Name,
    ChannelType Type,
    bool IsActive,
    DateTimeOffset CreatedAt,
    ChannelConfigResponse Config,
    int MonitorCount)
{
    public static NotificationChannelListItemResponse From(ChannelListRow row) => new(
        row.Channel.Id,
        row.Channel.Name,
        row.Channel.Type,
        row.Channel.IsActive,
        row.Channel.CreatedAt,
        ChannelConfigResponse.From(row.Channel.Config),
        row.MonitorCount);
}

public sealed record MonitorChannelResponse(
    Guid Id,
    string Name,
    ChannelType Type,
    bool IsActive,
    ChannelConfigResponse Config)
{
    public static MonitorChannelResponse From(NotificationChannel channel) => new(
        channel.Id,
        channel.Name,
        channel.Type,
        channel.IsActive,
        ChannelConfigResponse.From(channel.Config));
}

public sealed record ChannelMonitorResponse(Guid Id, string Name, MonitorType Type, string Target, bool IsActive);

public sealed record NotificationChannelResponse(
    Guid Id,
    string Name,
    ChannelType Type,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ChannelConfigResponse Config,
    IReadOnlyList<ChannelMonitorResponse> Monitors)
{
    public static NotificationChannelResponse From(NotificationChannelDetails details)
    {
        NotificationChannel channel = details.Channel;
        return new NotificationChannelResponse(
            channel.Id,
            channel.Name,
            channel.Type,
            channel.IsActive,
            channel.CreatedAt,
            channel.UpdatedAt,
            ChannelConfigResponse.From(channel.Config),
            details.Monitors
                .Select(m => new ChannelMonitorResponse(m.Id, m.Name, m.Type, m.Target, m.IsActive))
                .ToList());
    }
}
