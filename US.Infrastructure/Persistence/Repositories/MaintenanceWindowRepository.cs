using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<MaintenanceWindowListRow>> ListAsync(
        Guid organizationId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        return await db.MaintenanceWindows
            .AsNoTracking()
            .Where(w => w.OrganizationId == organizationId && w.DeletedAt == null)
            .OrderBy(w => w.Name)
            .Select(w => new MaintenanceWindowListRow(
                w,
                db.MaintenanceWindowMonitors.Count(m => m.MaintenanceWindowId == w.Id),
                db.MaintenanceOccurrences
                    .Where(o => o.MaintenanceWindowId == w.Id
                                && o.Status == MaintenanceOccurrenceStatus.Scheduled
                                && o.EndsAtUtc > now)
                    .OrderBy(o => o.StartsAtUtc)
                    .Select(o => new OccurrenceSlot(o.StartsAtUtc, o.EndsAtUtc))
                    .FirstOrDefault()))
            .ToListAsync(ct);
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
