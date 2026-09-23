using US.Domain.Enums;

namespace US.Application.Organizations.Commands.InviteMemberCommand;

public record InviteMemberCommand(Guid OrganizationId, string Email, OrganizationRole Role);