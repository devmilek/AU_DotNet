using FluentValidation;
using AU.Application.Exceptions;
using AU.Domain.Entities;

namespace AU.Application.Incidents.Commands.UpdateIncident;

public sealed record UpdateIncidentCommand(Guid OrganizationId, Guid IncidentId, string? Name, string? Cause);

public sealed class UpdateIncidentValidator : AbstractValidator<UpdateIncidentCommand>
{
    public UpdateIncidentValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.IncidentId).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(Incident.MaxNameLength);
        RuleFor(x => x.Cause).MaximumLength(Incident.MaxCauseLength);
    }
}

public sealed class UpdateIncidentHandler
{
    public async Task Handle(
        UpdateIncidentCommand command,
        IIncidentRepository incidents,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var incident = await incidents.GetAsync(command.OrganizationId, command.IncidentId, cancellationToken)
                       ?? throw new NotFoundException("Incident", command.IncidentId);

        incident.UpdateDetails(command.Name, command.Cause);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
