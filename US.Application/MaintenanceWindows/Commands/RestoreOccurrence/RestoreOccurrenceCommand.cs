using US.Application.Exceptions;

namespace US.Application.MaintenanceWindows.Commands.RestoreOccurrence;

public sealed record RestoreOccurrenceCommand(Guid OrganizationId, Guid WindowId, Guid OccurrenceId);

public sealed class RestoreOccurrenceHandler
{
    public async Task Handle(
        RestoreOccurrenceCommand command,
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
            throw new ConflictException("Termin wystąpienia już minął — nie można go przywrócić.");

        window.RestoreOccurrence(occurrence, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
