using Microsoft.Extensions.Logging;
using US.Application.Channels;
using US.Application.Notifications;
using US.Domain.Entities;
using US.Domain.ValueObjects;

namespace US.Infrastructure.Notifications;

public class NotificationSender(IEmailTemplateRenderer templateRenderer, IEmailSender emailSender, ILogger<NotificationSender> logger) : INotificationSender
{
    public async Task SendToChannelsAsync(IEnumerable<NotificationChannel> channels, string subject, string templateName, object templateModel)
    {
        foreach (var channel in channels.Where(c => c.IsActive))
        {
            try
            {
                switch (channel.Config)
                {
                    case EmailChannelConfig email:
                        var html = templateRenderer.Render(templateName, templateModel);
                        await emailSender.SendAsync(new EmailMessage(email.To, subject, html));
                        break;

                    // przyszłe: DiscordChannelConfig, WebhookChannelConfig
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Błąd wysyłki powiadomienia do kanału {ChannelId} ({ChannelName})",
                    channel.Id, channel.Name);
            }
        }
    }
}