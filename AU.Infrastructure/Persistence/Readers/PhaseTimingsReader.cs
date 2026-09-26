using Microsoft.EntityFrameworkCore;
using AU.Application.Checks.ReadModels;
using AU.Application.Checks.Statistics;
using AU.Domain.Enums;

namespace AU.Infrastructure.Persistence.Readers;

public sealed class PhaseTimingsReader(AppDbContext db) : IPhaseTimingsReader
{
    public async Task<IReadOnlyList<PhaseTimingsBucket>> GetPhaseBucketsAsync(
        Guid monitorId,
        DateTimeOffset from,
        TimeSpan bucketSize,
        ResponseTimeSource source,
        CancellationToken ct = default)
    {
        var buckets = source switch
        {
            ResponseTimeSource.RawChecks => await RawChecks(monitorId, from)
                .GroupBy(c => EF.Functions.TimeBucket(bucketSize, c.CheckedAt))
                .Select(g => new PhaseTimingsBucket(g.Key, new PhaseTimings(
                    g.Average(c => (double?)c.DnsMs),
                    g.Average(c => (double?)c.ConnectMs),
                    g.Average(c => (double?)c.TlsMs),
                    g.Average(c => (double?)c.TtfbMs),
                    g.Average(c => (double?)c.TransferMs))))
                .ToListAsync(ct),

            ResponseTimeSource.HourlyRollup => (await HourlyRollup(monitorId, from)
                    .GroupBy(h => EF.Functions.TimeBucket(bucketSize, h.Bucket))
                    .Select(g => new PhaseSums(
                        g.Key,
                        g.Sum(h => h.DnsCount), g.Sum(h => h.DnsSumMs),
                        g.Sum(h => h.ConnectCount), g.Sum(h => h.ConnectSumMs),
                        g.Sum(h => h.TlsCount), g.Sum(h => h.TlsSumMs),
                        g.Sum(h => h.TtfbCount), g.Sum(h => h.TtfbSumMs),
                        g.Sum(h => h.TransferCount), g.Sum(h => h.TransferSumMs)))
                    .ToListAsync(ct))
                .Select(sums => new PhaseTimingsBucket(sums.Bucket, sums.ToAverages()))
                .ToList(),

            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };

        return buckets.Where(b => b.Phases.HasAny).ToList();
    }

    public async Task<PhaseTimings?> GetPhaseAveragesAsync(
        Guid monitorId,
        DateTimeOffset from,
        ResponseTimeSource source,
        CancellationToken ct = default)
    {
        var averages = source switch
        {
            ResponseTimeSource.RawChecks => await RawChecks(monitorId, from)
                .GroupBy(_ => 1)
                .Select(g => new PhaseTimings(
                    g.Average(c => (double?)c.DnsMs),
                    g.Average(c => (double?)c.ConnectMs),
                    g.Average(c => (double?)c.TlsMs),
                    g.Average(c => (double?)c.TtfbMs),
                    g.Average(c => (double?)c.TransferMs)))
                .FirstOrDefaultAsync(ct),

            ResponseTimeSource.HourlyRollup => (await HourlyRollup(monitorId, from)
                    .GroupBy(_ => 1)
                    .Select(g => new PhaseSums(
                        from,
                        g.Sum(h => h.DnsCount), g.Sum(h => h.DnsSumMs),
                        g.Sum(h => h.ConnectCount), g.Sum(h => h.ConnectSumMs),
                        g.Sum(h => h.TlsCount), g.Sum(h => h.TlsSumMs),
                        g.Sum(h => h.TtfbCount), g.Sum(h => h.TtfbSumMs),
                        g.Sum(h => h.TransferCount), g.Sum(h => h.TransferSumMs)))
                    .FirstOrDefaultAsync(ct))
                ?.ToAverages(),

            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };

        return averages is { HasAny: true } ? averages : null;
    }

    private IQueryable<AU.Domain.Entities.Check> RawChecks(Guid monitorId, DateTimeOffset from) =>
        db.MonitorChecks.Where(c => c.MonitorId == monitorId && c.CheckedAt >= from && c.Status == CheckStatus.UP);

    private IQueryable<MonitorCheckPhasesHourly> HourlyRollup(Guid monitorId, DateTimeOffset from) =>
        db.MonitorCheckPhasesHourly.Where(h => h.MonitorId == monitorId && h.Bucket >= from);

    private sealed record PhaseSums(
        DateTimeOffset Bucket,
        long DnsCount, long? DnsSumMs,
        long ConnectCount, long? ConnectSumMs,
        long TlsCount, long? TlsSumMs,
        long TtfbCount, long? TtfbSumMs,
        long TransferCount, long? TransferSumMs)
    {
        public PhaseTimings ToAverages() => new(
            Average(DnsSumMs, DnsCount),
            Average(ConnectSumMs, ConnectCount),
            Average(TlsSumMs, TlsCount),
            Average(TtfbSumMs, TtfbCount),
            Average(TransferSumMs, TransferCount));

        private static double? Average(long? sum, long count) =>
            count > 0 && sum is { } total ? (double)total / count : null;
    }
}
