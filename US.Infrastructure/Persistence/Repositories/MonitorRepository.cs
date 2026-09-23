using Microsoft.EntityFrameworkCore;
using US.Application.Common;
using US.Application.Monitors;
using US.Application.Monitors.Queries.GetAllMonitors;
using US.Domain.Enums;
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
            .Include(x => x.State)
            .FirstOrDefaultAsync(
                x => x.Id == monitorId && x.OrganizationId == organizationId,
                ct);
    }

    public async Task<PagedResult<Monitor>> GetPagedAsync(
        GetAllMonitorsQuery query,
        CancellationToken ct = default)
    {
        var monitors = db.Monitors
            .Where(x => x.OrganizationId == query.OrganizationId);

        if (query.Types is { Count: > 0 } types)
        {
            monitors = monitors.Where(x => types.Contains(x.Type));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLikePattern(query.Search.Trim())}%";
            monitors = monitors.Where(x =>
                EF.Functions.ILike(x.Name, pattern, @"\") ||
                EF.Functions.ILike(x.Target, pattern, @"\"));
        }

        var totalCount = await monitors.CountAsync(ct);

        var ascending = query.SortOrder == SortOrder.Asc;

        // status monitora = czy ma otwarty (nierozwiązany) incydent; asc -> najpierw up, desc -> najpierw down
        var ordered = query.SortBy switch
        {
            MonitorSortBy.Status => ascending
                ? monitors.OrderBy(x => db.Incidents.Any(i => i.MonitorId == x.Id && i.Status != IncidentStatus.Resolved))
                : monitors.OrderByDescending(x => db.Incidents.Any(i => i.MonitorId == x.Id && i.Status != IncidentStatus.Resolved)),
            _ => ascending
                ? monitors.OrderBy(x => x.CreatedAt)
                : monitors.OrderByDescending(x => x.CreatedAt)
        };

        // stabilna kolejność między stronami
        var items = await ordered
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Monitor>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<Monitor?> GetForCheckAsync(
        Guid monitorId,
        CancellationToken ct = default)
    {
        return await db.Monitors
            .Include(x => x.State)
            .FirstOrDefaultAsync(
                x => x.Id == monitorId,
                ct);
    }

    public void Remove(Monitor monitor)
    {
        db.Monitors.Remove(monitor);
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
}
