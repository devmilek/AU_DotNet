using AU.Application.Abstractions;
using AU.Application.Organizations.Commands.CreateOrganization;

namespace AU.Application.Organizations.Queries.GetMyOrganizations;

public class GetMyOrganizationsHandler
{
    public async Task<MyOrganizationsResponse> Handle(GetMyOrganizationsQuery query, IOrganizationRepository organizationRepository, ICurrentUser currentUser, IFileStorage storage)
    {
        var organizations = await organizationRepository.GetForUserAsync(currentUser.UserId);

        var responses = organizations
            .Select(o => new OrganizationResponse(
                o.Id,
                o.Name,
                o.Slug,
                o.Members.First(m => m.UserId == currentUser.UserId).Role,
                storage.GetPublicUrlOrDefault(o.LogoKey)))
            .ToList();

        return new MyOrganizationsResponse(responses);
    }
}
