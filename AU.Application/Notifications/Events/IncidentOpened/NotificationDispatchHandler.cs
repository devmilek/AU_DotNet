using Microsoft.Extensions.Options;
using AU.Application.Channels;
using AU.Application.Incidents.Events.IncidentOpened;
using AU.Application.Monitors;
using AU.Application.Organizations;

namespace AU.Application.Notifications.Events.IncidentOpened;

public class NotificationDispatchHandler
{
    public async Task Handle(
        IncidentOpenedEvent @event,
        IMonitorNotificationChannelRepository channelRepository,
        IMonitorRepository monitorRepository,
        IOrganizationRepository organizationRepository,
        INotificationSender notificationSender,
        IOptions<FrontendOptions> frontendOptions)
    {
        var monitor = await monitorRepository.GetForCheckAsync(@event.MonitorId);
        if (monitor is null) return;

        var channels = await channelRepository.GetChannelsForMonitorAsync(@event.MonitorId);
        if (channels.Count == 0) return;

        var model = new IncidentOpenedModel(
            MonitorName: monitor.Name,
            MonitorTarget: monitor.Target,
            StartedAt: @event.OccurredAt.ToString("yyyy-MM-dd HH:mm"),
            FailedChecksCount: monitor.State.ConsecutiveFailures,
            MonitorUrl: await FrontendLinks.MonitorAsync(frontendOptions.Value.FrontendUrl, monitor, organizationRepository));

        // jedna wysyłka do wszystkich kanałów — sender sam pomija nieaktywne i wybiera sposób dostarczenia per typ
        await notificationSender.SendToChannelsAsync(
            channels,
            subject: $"🔴 {monitor.Name} nie odpowiada | Asterio Uptime",
            templateName: "incident-opened",
            templateModel: model);
    }
}
