using Microsoft.EntityFrameworkCore;
using US.Infrastructure.Persistence;

namespace US.CheckScheduler;

public class MonitorClaimer(AppDbContext db)
{
    /// <summary>
    /// Atomowo "zajmuje" monitory do sprawdzenia: wybiera zaległe, przesuwa im NextCheckAt o interval
    /// i zwraca ich id — jednym zapytaniem. FOR UPDATE SKIP LOCKED sprawia, że równoległe instancje
    /// schedulera nie czekają na siebie i nigdy nie dostaną tego samego monitora.
    /// </summary>
    public async Task<List<Guid>> ClaimDueMonitorsAsync(int batchSize, CancellationToken ct = default)
    {
        return await db.Database
            .SqlQuery<Guid>($"""
                WITH due AS (
                    SELECT s."MonitorId"
                    FROM monitor_states AS s
                    JOIN monitors AS m ON m."Id" = s."MonitorId"
                    WHERE m."IsActive" AND s."NextCheckAt" <= now()
                    ORDER BY s."NextCheckAt"
                    LIMIT {batchSize}
                    FOR UPDATE OF s SKIP LOCKED
                )
                UPDATE monitor_states AS s
                SET "NextCheckAt" = GREATEST(s."NextCheckAt", now()) + m."IntervalSeconds" * interval '1 second',
                    "UpdatedAt" = now()
                FROM due
                JOIN monitors AS m ON m."Id" = due."MonitorId"
                WHERE s."MonitorId" = due."MonitorId"
                RETURNING s."MonitorId" AS "Value"
                """)
            .ToListAsync(ct);
    }
}
