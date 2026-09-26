using AU.Domain.Enums;

namespace AU.Application.Organizations.Commands.InviteMemberCommand;

public sealed record InvitationResponse(
    Guid Id,
    string Email,
    OrganizationRole Role,
    DateTimeOffset ExpiresAt);
