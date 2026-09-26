using AU.Application.Organizations.Commands.CreateOrganization;

namespace AU.Application.Organizations.Queries.GetMyOrganizations;

public record MyOrganizationsResponse(IReadOnlyList<OrganizationResponse> Organizations);
