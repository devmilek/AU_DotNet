namespace US.Application.Notifications.Events.PasswordResetRequested;

public class PasswordResetEmailHandler
{
    public async Task Handle(
        PasswordResetRequestedEvent @event,
        IEmailTemplateRenderer templateRenderer,
        IEmailSender emailSender)
    {
        var html = templateRenderer.Render("password-reset",
            new PasswordResetEmailModel(
                DisplayName: @event.DisplayName ?? @event.Email,
                ResetUrl: @event.ResetUrl,
                ExpiresInMinutes: @event.ExpiresInMinutes));

        await emailSender.SendAsync(new EmailMessage(
            To: [@event.Email],
            Subject: "Resetowanie hasła | Asterio Uptime",
            HtmlBody: html));
    }
}
