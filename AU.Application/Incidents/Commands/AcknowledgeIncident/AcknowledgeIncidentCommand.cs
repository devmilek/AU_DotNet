using AU.Application.Abstractions;
using AU.Application.Exceptions;

namespace AU.Application.Incidents.Commands.AcknowledgeIncident;

public sealed record AcknowledgeIncidentCommand(Guid OrganizationId, Guid IncidentId);

public sealed class AcknowledgeIncidentHandler
{
    public async Task Handle(
        AcknowledgeIncidentCommand command,
        IIncidentRepository incidents,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var incident = await incidents.GetAsync(command.OrganizationId, command.IncidentId, cancellationToken)
                       ?? throw new NotFoundException("Incident", command.IncidentId);

        if (incident.IsResolved)
            throw new ConflictException("This incident is already resolved and can’t be acknowledged.");

        incident.Acknowledge(currentUser.UserId, timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
