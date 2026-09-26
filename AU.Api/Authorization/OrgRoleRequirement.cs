using Microsoft.AspNetCore.Authorization;
using AU.Domain.Enums;

namespace AU.Api.Authorization;

public class OrgRoleRequirement(OrganizationRole minimumRole) : IAuthorizationRequirement
{
    public OrganizationRole MinimumRole { get; } = minimumRole;
}