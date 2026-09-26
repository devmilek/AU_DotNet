using AU.Application.Exceptions;
using AU.Application.Monitors;
using AU.Domain.Entities;

namespace AU.Application.Channels.Queries.GetMonitorChannels;

public sealed record GetMonitorChannelsQuery(Guid OrganizationId, Guid MonitorId);

public sealed class GetMonitorChannelsHandler
{
    public async Task<IReadOnlyList<NotificationChannel>> Handle(
        GetMonitorChannelsQuery query,
        IMonitorRepository monitorRepository,
        IMonitorNotificationChannelRepository monitorChannels,
        CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(query.OrganizationId, query.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", query.MonitorId);

        var channels = await monitorChannels.GetChannelsForMonitorAsync(monitor.Id);

        return channels
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
