using AU.Application.Exceptions;

namespace AU.Application.MaintenanceWindows.Commands.UpdateOccurrenceContent;

public sealed record UpdateOccurrenceContentCommand(
    Guid OrganizationId,
    Guid WindowId,
    Guid OccurrenceId,
    string Name,
    string? Description);

public sealed class UpdateOccurrenceContentHandler
{
    public async Task Handle(
        UpdateOccurrenceContentCommand command,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetForOccurrenceAsync(
                         command.OrganizationId, command.WindowId, command.OccurrenceId, now, cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        var occurrence = window.FindOccurrence(command.OccurrenceId)
                         ?? throw new NotFoundException("Wystąpienie okna serwisowego", command.OccurrenceId);

        window.OverrideOccurrenceContent(occurrence, command.Name, command.Description, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
