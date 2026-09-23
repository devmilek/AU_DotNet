using US.Domain.Entities;

namespace US.Application.Channels;

public interface INotificationChannelRepository
{
    void Add(NotificationChannel notificationChannel);

    // ścieżka API — zawsze z organizacją
    Task<NotificationChannel?> GetAsync(Guid organizationId, Guid channelId, CancellationToken ct = default);
}
