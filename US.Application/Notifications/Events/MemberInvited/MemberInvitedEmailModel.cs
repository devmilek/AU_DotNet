namespace US.Application.Notifications.Events.MemberInvited;

public record MemberInvitedEmailModel(
    string OrganizationName,
    string InvitedByName,
    string Role,
    string InvitationUrl,
    string ExpiresAt);
