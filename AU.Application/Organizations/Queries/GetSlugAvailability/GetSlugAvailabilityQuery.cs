using AU.Domain;

namespace AU.Application.Organizations.Queries.GetSlugAvailability;

public sealed record GetSlugAvailabilityQuery(string Slug);

public sealed record SlugAvailability(string Slug, bool Available);

public sealed class GetSlugAvailabilityHandler
{
    public async Task<SlugAvailability> Handle(
        GetSlugAvailabilityQuery query,
        IOrganizationRepository organizations)
    {
        var slug = query.Slug.Trim();

        var available = OrganizationSlug.IsUsable(slug) && !await organizations.SlugExistsAsync(slug);

        return new SlugAvailability(slug, available);
    }
}
