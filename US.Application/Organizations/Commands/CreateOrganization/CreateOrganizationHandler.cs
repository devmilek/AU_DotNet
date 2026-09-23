using US.Application.Abstractions;
using US.Domain;
using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationHandler
{
    public async Task<OrganizationResponse> Handle(
        CreateOrganizationCommand command,
        IOrganizationRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        var slug = await GenerateUniqueSlugAsync(command.Name, repository);

        var organization = Organization.Create(command.Name, slug, currentUser.UserId);

        await repository.AddAsync(organization);
        await unitOfWork.SaveChangesAsync();

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.Slug,
            OrganizationRole.Owner);
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        IOrganizationRepository repository)
    {
        var baseSlug = Slugger.Create(name);

        if (!await repository.SlugExistsAsync(baseSlug))
            return baseSlug;

        return $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
    }
}