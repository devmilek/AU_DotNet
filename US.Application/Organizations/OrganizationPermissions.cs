using US.Application.Exceptions;
using US.Domain.Enums;

namespace US.Application.Organizations;

internal static class OrganizationPermissions
{
    public static async Task<OrganizationRole> RequireRoleAsync(
        IOrganizationMemberRepository members,
        Guid organizationId,
        Guid userId)
    {
        return await members.GetRoleAsync(organizationId, userId)
               ?? throw new ForbiddenException("You are not a member of this organization.");
    }

    public static void EnsureCanManage(OrganizationRole actorRole, OrganizationRole targetRole)
    {
        if (targetRole > actorRole)
            throw new ForbiddenException("You can’t manage members with a higher role than yours.");
    }

    public static void EnsureCanGrant(OrganizationRole actorRole, OrganizationRole role)
    {
        if (role > actorRole)
            throw new ForbiddenException("You can’t grant a role higher than your own.");
    }
}
