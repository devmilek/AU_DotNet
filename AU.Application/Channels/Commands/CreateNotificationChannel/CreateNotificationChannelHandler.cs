using AU.Application.Monitors;
using AU.Domain.Entities;
using AU.Domain.Enums;
using AU.Domain.ValueObjects;

namespace AU.Application.Channels.Commands.CreateNotificationChannel;

public sealed class CreateNotificationChannelHandler
{
    public async Task<Guid> Handle(
        CreateNotificationChannelCommand command,
        INotificationChannelRepository channelRepository,
        IMonitorNotificationChannelRepository linkRepository,
        IMonitorRepository monitorRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var channel = NotificationChannel.Create(command.OrganizationId, command.Name, ToConfig(command));
        channelRepository.Add(channel);

        var monitorIds = await MonitorAssignment.EnsureMonitorsExistAsync(
            monitorRepository, command.OrganizationId, command.MonitorIds, cancellationToken);
        foreach (var monitorId in monitorIds)
            linkRepository.Add(new MonitorNotificationChannel(monitorId, channel.Id));

        // kanał i przypięcia zapisują się razem albo wcale
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return channel.Id;
    }

    private static ChannelConfig ToConfig(CreateNotificationChannelCommand command) => command.Type switch
    {
        ChannelType.Email => EmailChannelConfig.Create(command.Email!.To),
        // walidator nie przepuści innych typów — ten wyjątek to tylko zabezpieczenie
        _ => throw new NotSupportedException($"Typ kanału {command.Type} nie jest jeszcze wspierany.")
    };
}
