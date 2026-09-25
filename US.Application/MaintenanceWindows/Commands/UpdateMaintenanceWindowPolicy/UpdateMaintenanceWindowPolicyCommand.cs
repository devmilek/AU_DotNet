using US.Application.Exceptions;

namespace US.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowPolicy;

public sealed record UpdateMaintenanceWindowPolicyCommand(
    Guid OrganizationId,
    Guid WindowId,
    bool SuppressNotifications,
    bool ExcludeFromSla);

public sealed class UpdateMaintenanceWindowPolicyHandler
{
    public async Task Handle(
        UpdateMaintenanceWindowPolicyCommand command,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetAsync(command.OrganizationId, command.WindowId, now, ct: cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        window.UpdatePolicy(command.SuppressNotifications, command.ExcludeFromSla, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
