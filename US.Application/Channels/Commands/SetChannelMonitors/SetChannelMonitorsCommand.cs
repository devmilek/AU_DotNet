using US.Application.Exceptions;
using US.Application.Monitors;
using US.Domain.Entities;

namespace US.Application.Channels.Commands.SetChannelMonitors;

/// <summary>Ustawia pełny zbiór monitorów przypiętych do kanału (PUT — idempotentne).</summary>
public sealed record SetChannelMonitorsCommand(Guid OrganizationId, Guid ChannelId, IReadOnlyList<Guid> MonitorIds);

public sealed class SetChannelMonitorsHandler
{
    public async Task Handle(
        SetChannelMonitorsCommand command,
        INotificationChannelRepository channelRepository,
        IMonitorNotificationChannelRepository linkRepository,
        IMonitorRepository monitorRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var channel = await channelRepository.GetAsync(command.OrganizationId, command.ChannelId, cancellationToken)
                      ?? throw new NotFoundException("Kanał powiadomień", command.ChannelId);

        var desired = await MonitorAssignment.EnsureMonitorsExistAsync(
            monitorRepository, command.OrganizationId, command.MonitorIds, cancellationToken);

        var current = await linkRepository.GetForChannelAsync(channel.Id, cancellationToken);
        var currentIds = current.Select(l => l.MonitorId).ToHashSet();

        // diff zamiast "usuń wszystko i dodaj od nowa" — nie rusza przypięć, które się nie zmieniły
        foreach (var link in current.Where(l => !desired.Contains(l.MonitorId)))
            linkRepository.Remove(link);

        foreach (var monitorId in desired.Where(id => !currentIds.Contains(id)))
            linkRepository.Add(new MonitorNotificationChannel(monitorId, channel.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
