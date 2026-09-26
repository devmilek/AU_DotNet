using AU.Application.Exceptions;

namespace AU.Application.MaintenanceWindows.Commands.RenameMaintenanceWindow;

public sealed record RenameMaintenanceWindowCommand(
    Guid OrganizationId,
    Guid WindowId,
    string Name,
    string? Description,
    bool ApplyToPastOccurrences = false);

public sealed class RenameMaintenanceWindowHandler
{
    public async Task Handle(
        RenameMaintenanceWindowCommand command,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetAsync(
                         command.OrganizationId, command.WindowId, now,
                         includePastOccurrences: command.ApplyToPastOccurrences, cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        window.Rename(command.Name, command.Description, command.ApplyToPastOccurrences, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
