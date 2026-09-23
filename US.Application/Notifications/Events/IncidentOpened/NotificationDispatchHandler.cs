using US.Application.Channels;
using US.Application.Incidents.Events.IncidentOpened;
using US.Application.Monitors;
using US.Domain.ValueObjects;

namespace US.Application.Notifications.Events.IncidentOpened;

public class NotificationDispatchHandler()
{
    public async Task Handle(IncidentOpenedEvent @event, IMonitorNotificationChannelRepository channelRepository,
        IMonitorRepository monitorRepository, INotificationSender notificationSender)
    {
        var monitor = await monitorRepository.GetForCheckAsync(@event.MonitorId);
        if (monitor is null) return;

        var channels = await channelRepository.GetChannelsForMonitorAsync(@event.MonitorId);

        foreach (var channel in channels.Where(c => c.IsActive))
        {
            if (channel.Config is not EmailChannelConfig email) continue;

            var model = new IncidentOpenedModel(
                MonitorName: monitor.Name,
                StartedAt: @event.OccurredAt.ToString("yyyy-MM-dd HH:mm"),
                FailedChecksCount: monitor.State.ConsecutiveFailures,
                MonitorUrl: $"https://app.example.com/monitors/{monitor.Id}"
            );

            await notificationSender.SendToChannelsAsync(
                channels,
                subject: $"🔴 {monitor.Name} nie odpowiada",
                templateName: "incident-opened",
                templateModel: model);
        }
    }
}