using Microsoft.EntityFrameworkCore;
using US.Application.Channels;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class NotificationChannelRepository(AppDbContext db) : INotificationChannelRepository
{
    public void Add(NotificationChannel notificationChannel)
    {
        db.NotificationChannels.Add(notificationChannel);
    }

    public async Task<NotificationChannel?> GetAsync(
        Guid organizationId,
        Guid channelId,
        CancellationToken ct = default)
    {
        return await db.NotificationChannels
            .FirstOrDefaultAsync(
                x => x.Id == channelId && x.OrganizationId == organizationId,
                ct);
    }

    public async Task<IReadOnlyList<ChannelListRow>> ListAsync(Guid organizationId, CancellationToken ct = default)
    {
        // liczba monitorów jako podzapytanie — jedno zapytanie zamiast N
        return await db.NotificationChannels
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .Select(x => new ChannelListRow(
                x,
                db.MonitorNotificationChannels.Count(l => l.NotificationChannelId == x.Id)))
            .ToListAsync(ct);
    }
}
