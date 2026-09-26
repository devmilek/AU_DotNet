using AU.Application.Channels;
using AU.Application.Exceptions;
using AU.Application.Monitors;

namespace AU.Application.MaintenanceWindows.Commands.SetMaintenanceWindowMonitors;

public sealed record SetMaintenanceWindowMonitorsCommand(Guid OrganizationId, Guid WindowId, IReadOnlyList<Guid> MonitorIds);

public sealed class SetMaintenanceWindowMonitorsHandler
{
    public async Task Handle(
        SetMaintenanceWindowMonitorsCommand command,
        IMaintenanceWindowRepository windowRepository,
        IMonitorRepository monitorRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetAsync(command.OrganizationId, command.WindowId, now, ct: cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        var monitorIds = await MonitorAssignment.EnsureMonitorsExistAsync(
            monitorRepository, command.OrganizationId, command.MonitorIds, cancellationToken);

        window.SetMonitors(monitorIds, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
