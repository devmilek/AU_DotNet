using Microsoft.EntityFrameworkCore;
using US.Application.Checks.ReadModels;
using US.Application.Checks.Statistics;
using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Infrastructure.Persistence.Readers;

public sealed class MonitorStatisticsReader(AppDbContext db) : IMonitorStatisticsReader
{
    public async Task<IReadOnlyList<MonitorCheckHourly>> GetHourlyAsync(
        IReadOnlyCollection<Guid> monitorIds,
        DateTimeOffset from,
        CancellationToken ct = default)
    {
        if (monitorIds.Count == 0) return [];

        return await db.MonitorChecksHourly
            .Where(x => monitorIds.Contains(x.MonitorId) && x.Bucket >= from)
            .ToListAsync(ct);
    }

    public async Task<Check?> GetLatestCheckAsync(Guid monitorId, CancellationToken ct = default)
    {
        // indeks (monitor_id, checked_at) + chunk exclusion — czyta tylko najnowszy chunk
        return await db.MonitorChecks
            .AsNoTracking()
            .Where(c => c.MonitorId == monitorId)
            .OrderByDescending(c => c.CheckedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CheckTotals> GetTotalsAsync(
        Guid monitorId,
        DateTimeOffset from,
        RollupGranularity granularity,
        CancellationToken ct = default)
    {
        IQueryable<MonitorCheckRollup> rollups = granularity switch
        {
            RollupGranularity.Hourly => db.MonitorChecksHourly,
            RollupGranularity.Daily => db.MonitorChecksDaily,
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
        };

        // GroupBy po stałej = jedno zapytanie z SUM zamiast pobierania wierszy
        var totals = await rollups
            .Where(x => x.MonitorId == monitorId && x.Bucket >= from)
            .GroupBy(_ => 1)
            .Select(g => new CheckTotals(g.Sum(x => x.TotalChecks), g.Sum(x => x.UpChecks)))
            .FirstOrDefaultAsync(ct);

        return totals ?? CheckTotals.Empty;
    }

    public async Task<IReadOnlyList<ResponseTimeBucket>> GetResponseTimeBucketsAsync(
        Guid monitorId,
        DateTimeOffset from,
        TimeSpan bucketSize,
        ResponseTimeSource source,
        CancellationToken ct = default)
    {
        return source switch
        {
            ResponseTimeSource.RawChecks => await db.MonitorChecks
                .Where(c => c.MonitorId == monitorId
                            && c.CheckedAt >= from
                            && c.Status == CheckStatus.UP
                            && c.ResponseTimeMs != null)
                .GroupBy(c => EF.Functions.TimeBucket(bucketSize, c.CheckedAt))
                .Select(g => new ResponseTimeBucket(
                    g.Key,
                    g.LongCount(),
                    g.Sum(c => (long)c.ResponseTimeMs!.Value),
                    g.Min(c => c.ResponseTimeMs!.Value),
                    g.Max(c => c.ResponseTimeMs!.Value)))
                .ToListAsync(ct),

            // agregat trzyma sumy i liczniki, więc przekubełkowanie (np. 1 h -> 6 h) jest dokładne
            ResponseTimeSource.HourlyRollup => await db.MonitorChecksHourly
                .Where(h => h.MonitorId == monitorId && h.Bucket >= from && h.ResponseTimeCount > 0)
                .GroupBy(h => EF.Functions.TimeBucket(bucketSize, h.Bucket))
                .Select(g => new ResponseTimeBucket(
                    g.Key,
                    g.Sum(h => h.ResponseTimeCount),
                    g.Sum(h => h.ResponseTimeSumMs!.Value),
                    g.Min(h => h.MinResponseTimeMs!.Value),
                    g.Max(h => h.MaxResponseTimeMs!.Value)))
                .ToListAsync(ct),

            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}
