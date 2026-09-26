using Microsoft.EntityFrameworkCore;
using AU.Application.Common;
using AU.Application.Incidents;
using AU.Domain.Enums;

namespace AU.Infrastructure.Persistence.Readers;

public sealed class IncidentReader(AppDbContext db) : IIncidentReader
{
    public async Task<PagedResult<IncidentRow>> GetPagedAsync(
        Guid organizationId,
        IncidentStatusFilter status,
        Guid? monitorId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var incidents = ForOrganization(organizationId);

        incidents = status switch
        {
            IncidentStatusFilter.Ongoing => incidents.Where(x => x.Incident.Status != IncidentStatus.Resolved),
            IncidentStatusFilter.Resolved => incidents.Where(x => x.Incident.Status == IncidentStatus.Resolved),
            _ => incidents
        };

        if (monitorId is { } id)
            incidents = incidents.Where(x => x.Incident.MonitorId == id);

        var totalCount = await incidents.CountAsync(ct);

        var items = await Project(incidents
                .OrderByDescending(x => x.Incident.StartedAt)
                .ThenByDescending(x => x.Incident.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync(ct);

        return new PagedResult<IncidentRow>(items, page, pageSize, totalCount);
    }

    public async Task<IncidentRow?> GetAsync(Guid organizationId, Guid incidentId, CancellationToken ct = default)
    {
        return await Project(ForOrganization(organizationId).Where(x => x.Incident.Id == incidentId))
            .FirstOrDefaultAsync(ct);
    }

    private IQueryable<IncidentWithMonitor> ForOrganization(Guid organizationId) =>
        from incident in db.Incidents.AsNoTracking()
        join monitor in db.Monitors.AsNoTracking() on incident.MonitorId equals monitor.Id
        where monitor.OrganizationId == organizationId
        select new IncidentWithMonitor { Incident = incident, MonitorName = monitor.Name, MonitorTarget = monitor.Target };

    private static IQueryable<IncidentRow> Project(IQueryable<IncidentWithMonitor> query) =>
        query.Select(x => new IncidentRow(
            x.Incident.Id,
            x.Incident.Name,
            x.Incident.Cause,
            x.Incident.MonitorId,
            x.MonitorName,
            x.MonitorTarget,
            x.Incident.Status,
            x.Incident.StartedAt,
            x.Incident.AcknowledgedAt,
            x.Incident.AcknowledgedByUserId,
            x.Incident.ResolvedAt,
            x.Incident.FailedChecksCount,
            x.Incident.StartedInMaintenance));

    private sealed class IncidentWithMonitor
    {
        public required AU.Domain.Entities.Incident Incident { get; init; }
        public required string MonitorName { get; init; }
        public required string MonitorTarget { get; init; }
    }
}
