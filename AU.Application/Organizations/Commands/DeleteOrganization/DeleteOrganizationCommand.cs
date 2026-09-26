using FluentValidation;
using FluentValidation.Results;
using AU.Application.Exceptions;

namespace AU.Application.Organizations.Commands.DeleteOrganization;

public sealed record DeleteOrganizationCommand(Guid OrganizationId, string ConfirmationName);

public sealed class DeleteOrganizationValidator : AbstractValidator<DeleteOrganizationCommand>
{
    public DeleteOrganizationValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.ConfirmationName).NotEmpty();
    }
}

public sealed class DeleteOrganizationHandler
{
    public async Task Handle(
        DeleteOrganizationCommand command,
        IOrganizationRepository organizations,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetAsync(command.OrganizationId)
                           ?? throw new NotFoundException("Organization", command.OrganizationId);

        if (!string.Equals(command.ConfirmationName.Trim(), organization.Name, StringComparison.Ordinal))
            throw new ValidationException(
                [new ValidationFailure(nameof(command.ConfirmationName), "The name doesn’t match the organization name.")]);

        await organizations.DeleteAsync(organization, cancellationToken);
    }
}
