using Microsoft.EntityFrameworkCore;
using AU.Domain.Entities;
using AU.Domain.Enums;

namespace AU.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository(AppDbContext db) : IIncidentRepository
{
    public async Task<Incident?> GetOpenForMonitorAsync(Guid monitorId)
    {
        return await db.Incidents
            .Where(i => i.MonitorId == monitorId && i.Status != IncidentStatus.Resolved)
            .OrderByDescending(i => i.StartedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Incident?> GetLastResolvedForMonitorAsync(Guid monitorId)
    {
        return await db.Incidents
            .Where(i => i.MonitorId == monitorId && i.Status == IncidentStatus.Resolved)
            .OrderByDescending(i => i.ResolvedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Incident?> GetAsync(Guid organizationId, Guid incidentId, CancellationToken ct = default)
    {
        return await db.Incidents
            .Where(i => i.Id == incidentId
                        && db.Monitors.Any(m => m.Id == i.MonitorId && m.OrganizationId == organizationId))
            .FirstOrDefaultAsync(ct);
    }

    public void Add(Incident incident)
    {
        db.Incidents.Add(incident);
    }

    public void Remove(Incident incident)
    {
        db.Incidents.Remove(incident);
    }
}