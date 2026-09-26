using AU.Application.Exceptions;
using AU.Application.Monitors;
using AU.Domain.Entities;

namespace AU.Application.Channels.Commands.AssignChannelToMonitor;

public class AssingChannelToMonitorHandler
{
    public async Task Handle(AssignChannelToMonitorCommand command, IMonitorRepository monitorRepository, INotificationChannelRepository channelRepository, IMonitorNotificationChannelRepository monitorNotificationChannelRepository, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken);
        if (monitor is null) throw new NotFoundException("Monitor", command.MonitorId);

        var channel = await channelRepository.GetAsync(command.OrganizationId, command.ChannelId, cancellationToken);
        if (channel is null) throw new NotFoundException("Kanał powiadomień", command.ChannelId);

        var alreadyExists = await monitorNotificationChannelRepository.ExistsAsync(monitor.Id, channel.Id);
        if (alreadyExists) throw new ConflictException($"Kanał {command.ChannelId} jest już przypięty do monitora {command.MonitorId}.");

        monitorNotificationChannelRepository.Add(new MonitorNotificationChannel(monitor.Id, channel.Id));
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
