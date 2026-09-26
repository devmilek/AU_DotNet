using AU.Domain.Enums;

namespace AU.Api.Controllers.Organizations.Requests;

public sealed record InviteMemberRequest(
    string Email,
    OrganizationRole Role);

public sealed record UpdateOrganizationRequest(string Name);

public sealed record ChangeMemberRoleRequest(OrganizationRole Role);

public sealed record DeleteOrganizationRequest(string ConfirmationName);
