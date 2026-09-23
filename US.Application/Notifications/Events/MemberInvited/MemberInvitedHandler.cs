using Microsoft.Extensions.Options;

namespace US.Application.Notifications.Events.MemberInvited;

public class MemberInvitedHandler
{
    public async Task Handle(
        MemberInvitedEvent @event,
        IEmailTemplateRenderer templateRenderer,
        IEmailSender emailSender,
        IOptions<FrontendOptions> options)
    {
        var url = $"{options.Value.FrontendUrl}/invite?token={@event.Token}";

        var html = templateRenderer.Render("invitation",
            new MemberInvitedEmailModel(
                OrganizationName: @event.OrganizationName,
                InvitedByName: @event.InvitedByName,
                Role: @event.Role,
                InvitationUrl: url,
                ExpiresAt: @event.ExpiresAt.ToString("yyyy-MM-dd HH:mm")));

        await emailSender.SendAsync(new EmailMessage(
            To: [@event.Email],
            Subject: $"Zaproszenie do organizacji {@event.OrganizationName} | Asterio Uptime",
            HtmlBody: html));
    }
}
