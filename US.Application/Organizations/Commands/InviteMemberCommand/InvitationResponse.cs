using US.Domain.Enums;

namespace US.Application.Organizations.Commands.InviteMemberCommand;

public sealed record InvitationResponse(
    Guid Id,
    string Email,
    OrganizationRole Role,
    DateTimeOffset ExpiresAt);
