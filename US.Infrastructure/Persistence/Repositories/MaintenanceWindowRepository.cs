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

    public async Task<MaintenanceWindow?> GetAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset now,
        bool includePastOccurrences = false,
        CancellationToken ct = default)
    {
        var query = db.MaintenanceWindows
            .Include(w => w.Monitors)
            .Where(w => w.Id == windowId && w.OrganizationId == organizationId && w.DeletedAt == null);

        query = includePastOccurrences
            ? query.Include(w => w.Occurrences)
            : query.Include(w => w.Occurrences.Where(o => o.EndsAtUtc > now || o.ScheduledStartUtc >= now));

        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
    }

    public void RemoveOccurrences(IEnumerable<MaintenanceOccurrence> occurrences)
    {
        db.MaintenanceOccurrences.RemoveRange(occurrences);
    }
}
