namespace US.Application.Notifications.Events.EmailConfirmationRequested;

public class EmailConfirmationHandler
{
    public async Task Handle(EmailConfirmationRequestedEvent @event, IEmailTemplateRenderer templateRenderer, IEmailSender emailSender)
    {
        var html = templateRenderer.Render("email-confirmation", 
                new EmailConfirmationEmailModel(
                DisplayName: @event.DisplayName ?? @event.Email,
                ConfirmationUrl: @event.ConfirmationUrl,
                ExpiresInHours: 24)
            );

        await emailSender.SendAsync(new EmailMessage(
            To: [@event.Email],
            Subject: "Potwierdź adres email | Asterio Uptime",
            HtmlBody: html));
    }
}