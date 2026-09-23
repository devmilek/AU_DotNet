using Microsoft.EntityFrameworkCore;
using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository(AppDbContext db) : IIncidentRepository
{
    public async Task<Incident?> GetOpenForMonitorAsync(Guid monitorId)
    {
        return await db.Incidents
            .Where(i => i.MonitorId == monitorId && i.Status != IncidentStatus.Resolved)
            .OrderByDescending(i => i.StartedAt)
            .FirstOrDefaultAsync();
    }

    public void Add(Incident incident)
    {
        db.Incidents.Add(incident);
    }
}