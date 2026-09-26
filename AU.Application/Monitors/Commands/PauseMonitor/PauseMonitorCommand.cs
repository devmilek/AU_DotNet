using AU.Application.Exceptions;

namespace AU.Application.Monitors.Commands.PauseMonitor;

public sealed record PauseMonitorCommand(Guid OrganizationId, Guid MonitorId);

public sealed class PauseMonitorHandler
{
    public async Task Handle(
        PauseMonitorCommand command,
        IMonitorRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitor = await repository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", command.MonitorId);

        // idempotentne — ponowna pauza nic nie zmienia
        monitor.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
