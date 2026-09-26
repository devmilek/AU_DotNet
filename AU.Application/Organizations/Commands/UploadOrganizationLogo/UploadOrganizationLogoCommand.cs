using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using AU.Application.Abstractions;
using AU.Application.Exceptions;

namespace AU.Application.Organizations.Commands.UploadOrganizationLogo;

public sealed record UploadOrganizationLogoCommand(Guid OrganizationId, Stream Content);

public sealed record OrganizationLogoResponse(string LogoUrl);

public sealed class UploadOrganizationLogoValidator : AbstractValidator<UploadOrganizationLogoCommand>
{
    public const long MaxSizeBytes = 2 * 1024 * 1024;

    public UploadOrganizationLogoValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Content.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxSizeBytes)
            .OverridePropertyName("File")
            .WithMessage($"The logo must be a non-empty file of at most {MaxSizeBytes / 1024 / 1024} MB.");
    }
}

public sealed class UploadOrganizationLogoHandler
{
    public async Task<OrganizationLogoResponse> Handle(
        UploadOrganizationLogoCommand command,
        IOrganizationRepository organizations,
        IUnitOfWork unitOfWork,
        IFileStorage storage,
        ILogger<UploadOrganizationLogoHandler> logger,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetAsync(command.OrganizationId)
                           ?? throw new NotFoundException("Organization", command.OrganizationId);

        var format = await LogoImageFormat.DetectAsync(command.Content, cancellationToken)
                     ?? throw new ValidationException(
                         [new ValidationFailure("File", "The logo must be a PNG, JPEG or WebP image.")]);

        var key = $"organizations/{organization.Id}/logo/{Guid.CreateVersion7():N}{format.Extension}";

        await storage.SaveAsync(key, command.Content, format.ContentType, cancellationToken);

        string? previousKey;
        try
        {
            previousKey = organization.ReplaceLogo(key);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }

        if (previousKey is not null)
            await storage.TryDeleteAsync(previousKey, logger);

        return new OrganizationLogoResponse(storage.GetPublicUrl(key));
    }
}
