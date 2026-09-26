using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using AU.Application.Notifications;

namespace AU.Infrastructure.Notifications;

public class SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message)
    {
        // główne style trafiają inline (Gmail/Outlook), a bloki oznaczone data-premailer="ignore"
        // (media queries: mobile i tryb ciemny, link do fontów) zostają nietknięte w <head>
        var inlined = PreMailer.Net.PreMailer.MoveCssInline(
            message.HtmlBody,
            removeStyleElements: true,
            ignoreElements: "[data-premailer=ignore]",
            useEmailFormatter: true);

        if (inlined.Warnings.Count > 0)
        {
            logger.LogWarning("Premailer warnings dla wiadomości {Subject}: {Warnings}",
                message.Subject, string.Join("; ", inlined.Warnings));
        }
        
        var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(options.Value.FromAddress));
        foreach (var to in message.To)
            mime.To.Add(MailboxAddress.Parse(to));
        mime.Subject = message.Subject;
        var body = new BodyBuilder { HtmlBody = inlined.Html };
        if (inlined.Html.Contains($"cid:{EmailAssets.LogoContentId}", StringComparison.Ordinal))
        {
            var logo = body.LinkedResources.Add("logo.png", EmailAssets.Logo.Value, new ContentType("image", "png"));
            logo.ContentId = EmailAssets.LogoContentId;
        }
        mime.Body = body.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            client.CheckCertificateRevocation = false;
            // Auto: SSL dla portu 465, STARTTLS jeśli serwer go oferuje (Resend 587), plain dla Mailpit
            await client.ConnectAsync(options.Value.Host, options.Value.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(options.Value.Username, options.Value.Password);
            await client.SendAsync(mime);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Błąd wysyłki wiadomości e-mail do {Recipients} (temat: {Subject})",
                string.Join(", ", message.To), message.Subject);
            throw;
        }
    }
}