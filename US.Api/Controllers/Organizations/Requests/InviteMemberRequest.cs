using US.Domain.Enums;

namespace US.Api.Controllers.Organizations.Requests;

public sealed record InviteMemberRequest(
    string Email,
    OrganizationRole Role);
