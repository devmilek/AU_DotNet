namespace US.Application.Organizations;

public sealed class InvitationOptions
{
    public const string SectionName = "Auth:Invitations";

    public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(7);
}
