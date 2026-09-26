using Microsoft.AspNetCore.Authorization;
using AU.Application.Abstractions;
using AU.Application.Organizations;

namespace AU.Api.Authorization;

public class OrgRoleHandler(
    IHttpContextAccessor accessor,
    IOrganizationMemberRepository members,
    ICurrentUser currentUser) : AuthorizationHandler<OrgRoleRequirement>
{
    public const string RoleItemKey = "OrganizationRole";

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OrgRoleRequirement requirement)
    {
        var http = accessor.HttpContext;
        if (http is null || !currentUser.IsAuthenticated)
            return; // brak Succeed = odmowa

        if (!http.Request.RouteValues.TryGetValue("orgId", out var raw)
            || !Guid.TryParse(raw?.ToString(), out var organizationId))
            return;

        var role = await members.GetRoleAsync(organizationId, currentUser.UserId);

        if (role is not null && role >= requirement.MinimumRole)
        {
            http.Items[RoleItemKey] = role;
            context.Succeed(requirement);
        }
    }
}