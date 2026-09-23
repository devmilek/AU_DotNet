using US.Application.Exceptions;
using US.Application.Monitors;
using US.Domain.Entities;

namespace US.Application.Channels.Commands.AssignChannelToMonitor;

public class AssingChannelToMonitorHandler
{
    public async Task Handle(AssignChannelToMonitorCommand command, IMonitorRepository monitorRepository, INotificationChannelRepository channelRepository, IMonitorNotificationChannelRepository monitorNotificationChannelRepository, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken);
        if (monitor is null) throw new NotFoundException("Monitor", command.MonitorId);

        var channel = await channelRepository.GetAsync(command.OrganizationId, command.ChannelId, cancellationToken);
        if (channel is null) throw new NotFoundException("Kanał powiadomień", command.ChannelId);

        var alreadyExists = await monitorNotificationChannelRepository.ExistsAsync(monitor.Id, channel.Id);
        if (alreadyExists) throw new InvalidOperationException($"Channel with id {command.ChannelId} is already assigned to monitor with id {command.MonitorId}.");

        monitorNotificationChannelRepository.Add(new MonitorNotificationChannel(monitor.Id, channel.Id));
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
