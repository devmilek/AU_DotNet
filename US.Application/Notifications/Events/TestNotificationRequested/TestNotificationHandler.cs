using US.Application.Channels;
using US.Application.Channels.Commands.SendTestNotification;
using US.Application.Monitors;

namespace US.Application.Notifications.Events.TestNotificationRequested;

public class TestNotificationHandler
{
    public async Task Handle(
        TestNotificationEvent @event,
        IMonitorRepository monitorRepository,
        IMonitorNotificationChannelRepository channelRepository,
        INotificationSender notificationSender)
    {
        var monitor = await monitorRepository.GetForCheckAsync(@event.MonitorId);
        if (monitor is null)
            throw new Exception($"Monitor {@event.MonitorId} nie istnieje.");

        var channels = await channelRepository.GetChannelsForMonitorAsync(@event.MonitorId);
        if (channels.Count == 0) return;

        if (channels.Count == 0)
            throw new InvalidOperationException("Monitor nie ma przypisanych żadnych kanałów powiadomień.");

        var model = new TestNotificationEmailModel(
            MonitorName: monitor.Name,
            SentAt: @event.RequestedAt.ToString("yyyy-MM-dd HH:mm")
        );

        await notificationSender.SendToChannelsAsync(
            channels, subject: $"🧪 Testowe powiadomienie — {monitor.Name} | Asterio Uptime", templateName: "test-notification", templateModel: model);
    }
}