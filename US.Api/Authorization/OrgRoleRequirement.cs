using Microsoft.AspNetCore.Authorization;
using US.Domain.Enums;

namespace US.Api.Authorization;

public class OrgRoleRequirement(OrganizationRole minimumRole) : IAuthorizationRequirement
{
    public OrganizationRole MinimumRole { get; } = minimumRole;
}