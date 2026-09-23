using US.Domain.Entities;

namespace US.Application.Channels;

public interface IMonitorNotificationChannelRepository
{
    Task<bool> ExistsAsync(Guid monitorId, Guid channelId);
    Task<MonitorNotificationChannel?> GetAsync(Guid monitorId, Guid channelId);
    Task<List<NotificationChannel>> GetChannelsForMonitorAsync(Guid monitorId);
    void Add(MonitorNotificationChannel link);
    void Remove(MonitorNotificationChannel link);
}