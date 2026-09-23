using US.Application.Exceptions;
using US.Application.Monitors;

namespace US.Application.Channels.Commands.UnassignChannelFromMonitor;

public class UnassignChannelFromMonitorHandler
{
    public async Task Handle(UnassingChannelFromMonitorCommand command, IMonitorRepository monitorRepository, IMonitorNotificationChannelRepository monitorNotificationChannelRepository, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken);
        if (monitor is null) throw new NotFoundException("Monitor", command.MonitorId);

        var linked = await monitorNotificationChannelRepository.GetAsync(monitor.Id, command.ChannelId);
        if (linked is null) throw new NotFoundException("Przypisanie kanału do monitora", command.ChannelId);

        monitorNotificationChannelRepository.Remove(linked);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
