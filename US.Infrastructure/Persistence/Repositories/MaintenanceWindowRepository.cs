using Microsoft.EntityFrameworkCore;
using US.Application.Channels;
using US.Application.MaintenanceWindows;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class MaintenanceWindowRepository(AppDbContext db) : IMaintenanceWindowRepository
{
    public void Add(MaintenanceWindow window)
    {
        db.MaintenanceWindows.Add(window);
    }

    public Task<MaintenanceWindow?> GetAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset now,
        bool includePastOccurrences = false,
        CancellationToken ct = default)
    {
        var query = db.MaintenanceWindows
            .Where(w => w.Id == windowId && w.OrganizationId == organizationId && w.DeletedAt == null);

        return LoadAsync(query, now, includePastOccurrences, ct);
    }

    public async Task<MaintenanceWindowDetails?> GetDetailsAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        var window = await db.MaintenanceWindows
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.Id == windowId && w.OrganizationId == organizationId && w.DeletedAt == null,
                ct);

        if (window is null) return null;

        var monitors = await db.MaintenanceWindowMonitors
            .Where(link => link.MaintenanceWindowId == windowId)
            .Join(db.Monitors, link => link.MonitorId, monitor => monitor.Id, (_, monitor) => monitor)
            .OrderBy(m => m.Name)
            .Select(m => new MonitorSummary(m.Id, m.Name, m.Type, m.Target, m.IsActive))
            .ToListAsync(ct);

        var nextOccurrence = await db.MaintenanceOccurrences
            .Where(o => o.MaintenanceWindowId == windowId
                        && o.Status == MaintenanceOccurrenceStatus.Scheduled
                        && o.EndsAtUtc > now)
            .OrderBy(o => o.StartsAtUtc)
            .Select(o => new OccurrenceSlot(o.StartsAtUtc, o.EndsAtUtc))
            .FirstOrDefaultAsync(ct);

        return new MaintenanceWindowDetails(window, monitors, nextOccurrence);
    }

    public async Task<IReadOnlyList<WindowOccurrenceRow>?> ListWindowOccurrencesAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        var window = await db.MaintenanceWindows
            .AsNoTracking()
            .Where(w => w.Id == windowId && w.OrganizationId == organizationId && w.DeletedAt == null)
            .Select(w => new { w.Name, w.Description })
            .FirstOrDefaultAsync(ct);

        if (window is null) return null;

        return await db.MaintenanceOccurrences
            .AsNoTracking()
            .Where(o => o.MaintenanceWindowId == windowId && o.StartsAtUtc < to && o.EndsAtUtc > from)
            .OrderBy(o => o.StartsAtUtc)
            .Select(o => new WindowOccurrenceRow(
                o.Id,
                o.Name,
                o.Description,
                o.StartsAtUtc,
                o.EndsAtUtc,
                o.ScheduledStartUtc,
                o.Status,
                o.ContentLockedAt == null,
                o.Name != window.Name || o.Description != window.Description))
            .ToListAsync(ct);
    }

    public async Task<MaintenanceWindow?> GetForOccurrenceAsync(
        Guid organizationId,
        Guid windowId,
        Guid occurrenceId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        return await db.MaintenanceWindows
            .Include(w => w.Occurrences.Where(o => o.Id == occurrenceId || o.EndsAtUtc > now || o.ScheduledStartUtc >= now))
            .Where(w => w.Id == windowId && w.OrganizationId == organizationId && w.DeletedAt == null)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<MaintenanceWindowListRow>> ListAsync(
        Guid organizationId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        var windows = await db.MaintenanceWindows
            .AsNoTracking()
            .Where(w => w.OrganizationId == organizationId && w.DeletedAt == null)
            .OrderBy(w => w.Name)
            .Select(w => new
            {
                Window = w,
                NextOccurrence = db.MaintenanceOccurrences
                    .Where(o => o.MaintenanceWindowId == w.Id
                                && o.Status == MaintenanceOccurrenceStatus.Scheduled
                                && o.EndsAtUtc > now)
                    .OrderBy(o => o.StartsAtUtc)
                    .Select(o => new OccurrenceSlot(o.StartsAtUtc, o.EndsAtUtc))
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var windowIds = windows.Select(w => w.Window.Id).ToList();

        var monitorsByWindow = (await db.MaintenanceWindowMonitors
                .Where(link => windowIds.Contains(link.MaintenanceWindowId))
                .Join(db.Monitors, link => link.MonitorId, monitor => monitor.Id,
                    (link, monitor) => new { link.MaintenanceWindowId, monitor.Id, monitor.Name })
                .OrderBy(x => x.Name)
                .ToListAsync(ct))
            .ToLookup(x => x.MaintenanceWindowId, x => new WindowMonitorName(x.Id, x.Name));

        return windows
            .Select(w => new MaintenanceWindowListRow(
                w.Window,
                monitorsByWindow[w.Window.Id].ToList(),
                w.NextOccurrence))
            .ToList();
    }

    public async Task<IReadOnlyList<MaintenanceOccurrenceRow>> ListOccurrencesAsync(
        Guid organizationId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        return await db.MaintenanceOccurrences
            .AsNoTracking()
            .Where(o => o.Status == MaintenanceOccurrenceStatus.Scheduled
                        && o.StartsAtUtc < to
                        && o.EndsAtUtc > from
                        && db.MaintenanceWindows.Any(w => w.Id == o.MaintenanceWindowId
                                                          && w.OrganizationId == organizationId
                                                          && w.DeletedAt == null))
            .OrderBy(o => o.StartsAtUtc)
            .Select(o => new MaintenanceOccurrenceRow(
                o.Id,
                o.MaintenanceWindowId!.Value,
                o.Name,
                o.StartsAtUtc,
                o.EndsAtUtc))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetRecurringIdsAsync(CancellationToken ct = default)
    {
        return await db.MaintenanceWindows
            .Where(w => w.DeletedAt == null && w.RecurrenceRule != null)
            .OrderBy(w => w.Id)
            .Select(w => w.Id)
            .ToListAsync(ct);
    }

    public Task<MaintenanceWindow?> GetForSchedulingAsync(Guid windowId, DateTimeOffset now, CancellationToken ct = default)
    {
        var query = db.MaintenanceWindows
            .Where(w => w.Id == windowId && w.DeletedAt == null);

        return LoadAsync(query, now, includePastOccurrences: false, ct);
    }

    private static Task<MaintenanceWindow?> LoadAsync(
        IQueryable<MaintenanceWindow> query,
        DateTimeOffset now,
        bool includePastOccurrences,
        CancellationToken ct)
    {
        query = query.Include(w => w.Monitors);

        query = includePastOccurrences
            ? query.Include(w => w.Occurrences)
            : query.Include(w => w.Occurrences.Where(o => o.EndsAtUtc > now || o.ScheduledStartUtc >= now));

        return query
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
    }

    public void RemoveOccurrences(IEnumerable<MaintenanceOccurrence> occurrences)
    {
        db.MaintenanceOccurrences.RemoveRange(occurrences);
    }
}
