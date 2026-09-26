using AU.Application.Channels;
using AU.Application.Channels.Commands.SendTestNotification;
using Microsoft.Extensions.Options;
using AU.Application.Monitors;
using AU.Application.Organizations;

namespace AU.Application.Notifications.Events.TestNotificationRequested;

public class TestNotificationHandler
{
    public async Task Handle(
        TestNotificationEvent @event,
        IMonitorRepository monitorRepository,
        IMonitorNotificationChannelRepository channelRepository,
        IOrganizationRepository organizationRepository,
        INotificationSender notificationSender,
        IOptions<FrontendOptions> frontendOptions)
    {
        var monitor = await monitorRepository.GetForCheckAsync(@event.MonitorId);
        if (monitor is null)
            throw new InvalidOperationException($"Monitor {@event.MonitorId} nie istnieje.");

        var channels = await channelRepository.GetChannelsForMonitorAsync(@event.MonitorId);
        if (channels.Count == 0) return;

        var model = new TestNotificationEmailModel(
            MonitorName: monitor.Name,
            SentAt: @event.RequestedAt.ToString("yyyy-MM-dd HH:mm"),
            MonitorUrl: await FrontendLinks.MonitorAsync(frontendOptions.Value.FrontendUrl, monitor, organizationRepository));

        await notificationSender.SendToChannelsAsync(
            channels,
            subject: $"🧪 Testowe powiadomienie — {monitor.Name} | Asterio Uptime",
            templateName: "test-notification",
            templateModel: model);
    }
}
