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
