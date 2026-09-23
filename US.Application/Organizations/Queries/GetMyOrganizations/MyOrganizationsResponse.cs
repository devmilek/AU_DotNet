using US.Application.Organizations.Commands.CreateOrganization;

namespace US.Application.Organizations.Queries.GetMyOrganizations;

public record MyOrganizationsResponse(IReadOnlyList<OrganizationResponse> Organizations);
