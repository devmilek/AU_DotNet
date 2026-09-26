namespace AU.Application.Notifications.Events.MemberInvited;

public record MemberInvitedEvent(
    string Email,
    string OrganizationName,
    string InvitedByName,
    string Role,
    string Token,
    DateTimeOffset ExpiresAt);
