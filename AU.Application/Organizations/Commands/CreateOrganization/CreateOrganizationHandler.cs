using AU.Application.Abstractions;
using AU.Domain;
using AU.Domain.Entities;
using AU.Domain.Enums;

namespace AU.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationHandler
{
    public async Task<OrganizationResponse> Handle(
        CreateOrganizationCommand command,
        IOrganizationRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        var slug = await ResolveUniqueSlugAsync(
            command.Slug ?? OrganizationSlug.FromName(command.Name), repository);

        var organization = Organization.Create(command.Name, slug, currentUser.UserId);

        await repository.AddAsync(organization);
        await unitOfWork.SaveChangesAsync();

        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.Slug,
            OrganizationRole.Owner,
            null);
    }

    private static async Task<string> ResolveUniqueSlugAsync(
        string requestedSlug,
        IOrganizationRepository repository)
    {
        if (OrganizationSlug.IsUsable(requestedSlug) && !await repository.SlugExistsAsync(requestedSlug))
            return requestedSlug;

        string slug;
        do
        {
            slug = OrganizationSlug.WithRandomSuffix(requestedSlug);
        } while (await repository.SlugExistsAsync(slug));

        return slug;
    }
}
