using Microsoft.EntityFrameworkCore;
using US.Infrastructure.Persistence;

namespace US.CheckScheduler;

public class MonitorClaimer(AppDbContext db)
{
    public async Task<List<Guid>> ClaimDueMonitorsAsync(int batchSize)
    {
        var now = DateTimeOffset.UtcNow;
        var claimToken = Guid.NewGuid();

        var candidateIds = await db.Monitors
            .Where(m => m.IsActive && m.NextCheckAt <= now)
            .OrderBy(m => m.NextCheckAt)
            .Take(batchSize)
            .Select(m => m.Id)
            .ToListAsync();
        
        if (candidateIds.Count == 0) return [];

        await db.Monitors
            .Where(m => candidateIds.Contains(m.Id) && m.NextCheckAt <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.NextCheckAt, m => 
                    (m.NextCheckAt > now ? m.NextCheckAt : now).AddSeconds(m.IntervalSeconds))
                .SetProperty(m => m.ClaimToken, claimToken));

        return await db.Monitors
            .Where(m => m.ClaimToken == claimToken)
            .Select(m => m.Id)
            .ToListAsync();
    }
}