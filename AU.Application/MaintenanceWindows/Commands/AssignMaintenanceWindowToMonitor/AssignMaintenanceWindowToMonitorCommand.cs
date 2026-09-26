using AU.Application.Exceptions;
using AU.Application.Monitors;

namespace AU.Application.MaintenanceWindows.Commands.AssignMaintenanceWindowToMonitor;

public sealed record AssignMaintenanceWindowToMonitorCommand(Guid OrganizationId, Guid MonitorId, Guid WindowId);

public sealed class AssignMaintenanceWindowToMonitorHandler
{
    public async Task Handle(
        AssignMaintenanceWindowToMonitorCommand command,
        IMaintenanceWindowRepository windowRepository,
        IMonitorRepository monitorRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var monitor = await monitorRepository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", command.MonitorId);

        var window = await windowRepository.GetAsync(command.OrganizationId, command.WindowId, now, ct: cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        if (!window.AssignMonitor(monitor.Id, now))
            throw new ConflictException($"Okno serwisowe {command.WindowId} jest już przypisane do monitora {command.MonitorId}.");

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
