using AU.Domain.Entities;

namespace AU.Application.Channels;

public interface INotificationSender
{
    Task SendToChannelsAsync(IEnumerable<NotificationChannel> channels, string subject, string templateName, object templateModel);
}