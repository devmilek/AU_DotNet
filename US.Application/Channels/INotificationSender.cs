using US.Domain.Entities;

namespace US.Application.Channels;

public interface INotificationSender
{
    Task SendToChannelsAsync(IEnumerable<NotificationChannel> channels, string subject, string templateName, object templateModel);
}