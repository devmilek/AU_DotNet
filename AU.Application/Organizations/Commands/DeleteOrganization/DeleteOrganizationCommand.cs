using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using AU.Application.Abstractions;
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
        IFileStorage storage,
        ILogger<DeleteOrganizationHandler> logger,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetAsync(command.OrganizationId)
                           ?? throw new NotFoundException("Organization", command.OrganizationId);

        if (!string.Equals(command.ConfirmationName.Trim(), organization.Name, StringComparison.Ordinal))
            throw new ValidationException(
                [new ValidationFailure(nameof(command.ConfirmationName), "The name doesn’t match the organization name.")]);

        var logoKey = organization.LogoKey;

        await organizations.DeleteAsync(organization, cancellationToken);

        if (logoKey is not null)
            await storage.TryDeleteAsync(logoKey, logger);
    }
}
