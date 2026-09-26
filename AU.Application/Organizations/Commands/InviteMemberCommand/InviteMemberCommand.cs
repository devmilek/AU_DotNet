using AU.Domain.Enums;

namespace AU.Application.Organizations.Commands.InviteMemberCommand;

public record InviteMemberCommand(Guid OrganizationId, string Email, OrganizationRole Role);