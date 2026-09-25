using US.Application.Exceptions;

namespace US.Application.MaintenanceWindows.Commands.CancelOccurrence;

public sealed record CancelOccurrenceCommand(Guid OrganizationId, Guid WindowId, Guid OccurrenceId);

public sealed class CancelOccurrenceHandler
{
    public async Task Handle(
        CancelOccurrenceCommand command,
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
            throw new ConflictException("Wystąpienie już się rozpoczęło — nie można go anulować.");

        window.CancelOccurrence(occurrence, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
