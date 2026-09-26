using AU.Application.Exceptions;

namespace AU.Application.Incidents.Commands.DeleteIncident;

public sealed record DeleteIncidentCommand(Guid OrganizationId, Guid IncidentId);

public sealed class DeleteIncidentHandler
{
    public async Task Handle(
        DeleteIncidentCommand command,
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var incident = await incidents.GetAsync(command.OrganizationId, command.IncidentId, cancellationToken)
                       ?? throw new NotFoundException("Incident", command.IncidentId);

        if (!incident.IsResolved)
            throw new ConflictException("Ongoing incidents can’t be deleted. Wait until the monitor recovers.");

        incidents.Remove(incident);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
