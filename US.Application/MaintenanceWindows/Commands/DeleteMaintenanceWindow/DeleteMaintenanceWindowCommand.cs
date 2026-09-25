using US.Application.Exceptions;

namespace US.Application.MaintenanceWindows.Commands.DeleteMaintenanceWindow;

public sealed record DeleteMaintenanceWindowCommand(Guid OrganizationId, Guid WindowId);

public sealed class DeleteMaintenanceWindowHandler
{
    public async Task Handle(
        DeleteMaintenanceWindowCommand command,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetAsync(command.OrganizationId, command.WindowId, now, ct: cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        var futureOccurrences = window.Occurrences.Where(o => !o.HasStarted(now)).ToList();

        window.Delete(now);
        windowRepository.RemoveOccurrences(futureOccurrences);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
