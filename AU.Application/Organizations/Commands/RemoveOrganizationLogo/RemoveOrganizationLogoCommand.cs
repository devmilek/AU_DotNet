using FluentValidation;
using Microsoft.Extensions.Logging;
using AU.Application.Abstractions;
using AU.Application.Exceptions;

namespace AU.Application.Organizations.Commands.RemoveOrganizationLogo;

public sealed record RemoveOrganizationLogoCommand(Guid OrganizationId);

public sealed class RemoveOrganizationLogoValidator : AbstractValidator<RemoveOrganizationLogoCommand>
{
    public RemoveOrganizationLogoValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
    }
}

public sealed class RemoveOrganizationLogoHandler
{
    public async Task Handle(
        RemoveOrganizationLogoCommand command,
        IOrganizationRepository organizations,
        IUnitOfWork unitOfWork,
        IFileStorage storage,
        ILogger<RemoveOrganizationLogoHandler> logger,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetAsync(command.OrganizationId)
                           ?? throw new NotFoundException("Organization", command.OrganizationId);

        var previousKey = organization.RemoveLogo();
        if (previousKey is null)
            return;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await storage.TryDeleteAsync(previousKey, logger);
    }
}
