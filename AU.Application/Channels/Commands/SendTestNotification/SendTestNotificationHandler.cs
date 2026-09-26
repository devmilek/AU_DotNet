using AU.Application.Exceptions;
using AU.Application.Monitors;
using AU.Application.Notifications.Events.TestNotificationRequested;
using Wolverine;

namespace AU.Application.Channels.Commands.SendTestNotification;

public class SendTestNotificationHandler
{
    public async Task Handle(
        SendTestNotificationCommand cmd,
        IMonitorRepository monitorRepository,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(cmd.OrganizationId, cmd.MonitorId, cancellationToken);
        if (monitor is null)
            throw new NotFoundException("Monitor", cmd.MonitorId);

        await bus.PublishAsync(new TestNotificationEvent(monitor.Id, DateTimeOffset.UtcNow));
    }
}
