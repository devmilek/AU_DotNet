using FluentValidation;
using US.Application.Exceptions;
using US.Domain.Entities;

namespace US.Application.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(Guid OrganizationId, string Name);

public sealed class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Organization.MaxNameLength);
    }
}

public sealed class UpdateOrganizationHandler
{
    public async Task Handle(
        UpdateOrganizationCommand command,
        IOrganizationRepository organizations,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetAsync(command.OrganizationId)
                           ?? throw new NotFoundException("Organization", command.OrganizationId);

        organization.Rename(command.Name);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
