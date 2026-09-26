using AU.Application.Checks.Statistics;
using AU.Application.Exceptions;

namespace AU.Application.Monitors.Queries.GetMonitorResponseTimes;

public sealed class GetMonitorResponseTimesHandler
{
    /// <summary>
    /// Rozdzielczość per zakres: ~100-170 punktów na wykres. 24 h liczymy z surowych checków
    /// (dokładniej niż pełne godziny), dłuższe zakresy z agregatu godzinowego.
    /// </summary>
    private static readonly IReadOnlyDictionary<ResponseTimeRange, (TimeSpan Length, TimeSpan Bucket, ResponseTimeSource Source)> Ranges =
        new Dictionary<ResponseTimeRange, (TimeSpan, TimeSpan, ResponseTimeSource)>
        {
            [ResponseTimeRange.Last24Hours] = (TimeSpan.FromHours(24), TimeSpan.FromMinutes(15), ResponseTimeSource.RawChecks),
            [ResponseTimeRange.Last7Days] = (TimeSpan.FromDays(7), TimeSpan.FromHours(1), ResponseTimeSource.HourlyRollup),
            [ResponseTimeRange.Last30Days] = (TimeSpan.FromDays(30), TimeSpan.FromHours(6), ResponseTimeSource.HourlyRollup)
        };

    public async Task<MonitorResponseTimesResult> Handle(
        GetMonitorResponseTimesQuery query,
        IMonitorRepository monitorRepository,
        IMonitorStatisticsReader statistics,
        IPhaseTimingsReader phaseTimings,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(query.OrganizationId, query.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", query.MonitorId);

        var (length, bucketSize, source) = Ranges[query.Range];
        var bucketCount = (int)(length / bucketSize);

        // ostatni kubełek to bieżący (w toku), więc okno kończy się "teraz"
        var from = TimeWindows.Floor(timeProvider.GetUtcNow(), bucketSize) - bucketSize * (bucketCount - 1);

        var buckets = await statistics.GetResponseTimeBucketsAsync(monitor.Id, from, bucketSize, source, cancellationToken);
        var byTimestamp = buckets.ToDictionary(b => b.Bucket);

        var phaseBuckets = await phaseTimings.GetPhaseBucketsAsync(monitor.Id, from, bucketSize, source, cancellationToken);
        var phasesByTimestamp = phaseBuckets.ToDictionary(b => b.Bucket, b => b.Phases);
        var phaseSummary = await phaseTimings.GetPhaseAveragesAsync(monitor.Id, from, source, cancellationToken);

        var points = Enumerable.Range(0, bucketCount)
            .Select(i =>
            {
                var timestamp = from + bucketSize * i;
                var phases = phasesByTimestamp.GetValueOrDefault(timestamp);
                return byTimestamp.TryGetValue(timestamp, out var b)
                    ? new ResponseTimePoint(timestamp, (double)b.SumMs / b.Count, b.MinMs, b.MaxMs, phases)
                    : new ResponseTimePoint(timestamp, null, null, null, phases);
            })
            .ToList();

        return new MonitorResponseTimesResult(
            query.Range,
            from,
            (int)bucketSize.TotalSeconds,
            points,
            Summarize(buckets, phaseSummary));
    }

    // średnia ważona liczbą checków — średnia ze średnich kubełków byłaby przekłamana
    private static ResponseTimeSummary Summarize(IReadOnlyList<ResponseTimeBucket> buckets, PhaseTimings? phases)
    {
        var count = buckets.Sum(b => b.Count);
        if (count == 0) return new ResponseTimeSummary(null, null, null, phases);

        return new ResponseTimeSummary(
            (double)buckets.Sum(b => b.SumMs) / count,
            buckets.Min(b => b.MinMs),
            buckets.Max(b => b.MaxMs),
            phases);
    }
}
