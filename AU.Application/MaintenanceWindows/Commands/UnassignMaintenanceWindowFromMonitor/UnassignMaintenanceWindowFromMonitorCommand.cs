using AU.Application.Exceptions;

namespace AU.Application.MaintenanceWindows.Commands.UnassignMaintenanceWindowFromMonitor;

public sealed record UnassignMaintenanceWindowFromMonitorCommand(Guid OrganizationId, Guid MonitorId, Guid WindowId);

public sealed class UnassignMaintenanceWindowFromMonitorHandler
{
    public async Task Handle(
        UnassignMaintenanceWindowFromMonitorCommand command,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetAsync(command.OrganizationId, command.WindowId, now, ct: cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        if (!window.UnassignMonitor(command.MonitorId, now))
            throw new NotFoundException("Przypisanie okna serwisowego do monitora", command.MonitorId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
