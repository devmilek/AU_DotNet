using AU.Application.Exceptions;
using AU.Domain.Entities;

namespace AU.Application.Channels.Queries.GetNotificationChannel;

public sealed record GetNotificationChannelQuery(Guid OrganizationId, Guid ChannelId);

public sealed record NotificationChannelDetails(NotificationChannel Channel, IReadOnlyList<MonitorSummary> Monitors);

public sealed class GetNotificationChannelHandler
{
    public async Task<NotificationChannelDetails> Handle(
        GetNotificationChannelQuery query,
        INotificationChannelRepository channelRepository,
        IMonitorNotificationChannelRepository linkRepository,
        CancellationToken cancellationToken)
    {
        var channel = await channelRepository.GetAsync(query.OrganizationId, query.ChannelId, cancellationToken)
                      ?? throw new NotFoundException("Kanał powiadomień", query.ChannelId);

        var monitors = await linkRepository.GetMonitorsForChannelAsync(channel.Id, cancellationToken);
        return new NotificationChannelDetails(channel, monitors);
    }
}
