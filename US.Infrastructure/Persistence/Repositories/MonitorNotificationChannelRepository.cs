using Microsoft.EntityFrameworkCore;
using US.Application.Channels;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Repositories;

public class MonitorNotificationChannelRepository(AppDbContext db) : IMonitorNotificationChannelRepository
{
    public Task<bool> ExistsAsync(Guid monitorId, Guid channelId)
    {
        return db.MonitorNotificationChannels.AnyAsync(x => x.MonitorId == monitorId && x.NotificationChannelId == channelId);
    }

    public Task<MonitorNotificationChannel?> GetAsync(Guid monitorId, Guid channelId)
    {
        return db.MonitorNotificationChannels.FirstOrDefaultAsync(x => x.MonitorId == monitorId && x.NotificationChannelId == channelId);
    }

    public Task<List<NotificationChannel>> GetChannelsForMonitorAsync(Guid monitorId)
    {
        return db.MonitorNotificationChannels
            .Where(x => x.MonitorId == monitorId)
            .Join(db.NotificationChannels,
                link => link.NotificationChannelId,
                channel => channel.Id,
                (link, channel) => channel)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MonitorNotificationChannel>> GetForChannelAsync(
        Guid channelId,
        CancellationToken ct = default)
    {
        return await db.MonitorNotificationChannels
            .Where(x => x.NotificationChannelId == channelId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MonitorSummary>> GetMonitorsForChannelAsync(
        Guid channelId,
        CancellationToken ct = default)
    {
        return await db.MonitorNotificationChannels
            .Where(x => x.NotificationChannelId == channelId)
            .Join(db.Monitors, link => link.MonitorId, monitor => monitor.Id, (_, monitor) => monitor)
            .OrderBy(m => m.Name)
            .Select(m => new MonitorSummary(m.Id, m.Name, m.Type, m.Target, m.IsActive))
            .ToListAsync(ct);
    }

    public void Add(MonitorNotificationChannel link)
    {
        db.MonitorNotificationChannels.Add(link);
    }

    public void Remove(MonitorNotificationChannel link)
    {
        db.MonitorNotificationChannels.Remove(link);
    }
}