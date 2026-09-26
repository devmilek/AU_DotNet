using AU.Application.Exceptions;

namespace AU.Application.MaintenanceWindows.Commands.ResetOccurrenceContent;

public sealed record ResetOccurrenceContentCommand(Guid OrganizationId, Guid WindowId, Guid OccurrenceId);

public sealed class ResetOccurrenceContentHandler
{
    public async Task Handle(
        ResetOccurrenceContentCommand command,
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

        if (occurrence.HasStarted(now))
            throw new ConflictException("Wystąpienie już się rozpoczęło — jego treść można tylko edytować, nie przywrócić.");

        window.ResetOccurrenceContent(occurrence, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
