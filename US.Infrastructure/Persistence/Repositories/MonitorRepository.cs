using Microsoft.EntityFrameworkCore;
using US.Application.Monitors;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class MonitorRepository(AppDbContext db) : IMonitorRepository
{
    public async Task AddAsync(
        Monitor monitor,
        CancellationToken ct = default)
    {
        await db.Monitors.AddAsync(monitor, ct);
    }

    public async Task<Monitor?> GetAsync(
        Guid organizationId,
        Guid monitorId,
        CancellationToken ct = default)
    {
        return await db.Monitors
            .FirstOrDefaultAsync(
                x => x.Id == monitorId && x.OrganizationId == organizationId,
                ct);
    }

    public async Task<IReadOnlyList<Monitor>> GetAllAsync(
        Guid organizationId,
        CancellationToken ct = default)
    {
        return await db.Monitors
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<Monitor?> GetForCheckAsync(
        Guid monitorId,
        CancellationToken ct = default)
    {
        return await db.Monitors
            .FirstOrDefaultAsync(
                x => x.Id == monitorId,
                ct);
    }

    public void Remove(Monitor monitor)
    {
        db.Monitors.Remove(monitor);
    }
}
