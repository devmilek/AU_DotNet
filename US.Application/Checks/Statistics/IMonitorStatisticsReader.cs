using US.Application.Checks.ReadModels;
using US.Domain.Entities;

namespace US.Application.Checks.Statistics;

/// <summary>
/// Strona odczytu statystyk checków (surowa hypertabela i agregaty ciągłe).
/// Oddzielona od repozytoriów zapisu — nic tu nie modyfikuje stanu.
/// </summary>
public interface IMonitorStatisticsReader
{
    /// <summary>Godzinowe agregaty wielu monitorów naraz, od <paramref name="from"/> (włącznie).</summary>
    Task<IReadOnlyList<MonitorCheckHourly>> GetHourlyAsync(
        IReadOnlyCollection<Guid> monitorIds,
        DateTimeOffset from,
        CancellationToken ct = default);

    Task<Check?> GetLatestCheckAsync(Guid monitorId, CancellationToken ct = default);

    /// <summary>Suma checków i checków UP od <paramref name="from"/> z agregatu o danej ziarnistości.</summary>
    Task<CheckTotals> GetTotalsAsync(
        Guid monitorId,
        DateTimeOffset from,
        RollupGranularity granularity,
        CancellationToken ct = default);

    /// <summary>
    /// Czasy odpowiedzi udanych checków pogrupowane w kubełki <paramref name="bucketSize"/>.
    /// Zwraca tylko kubełki, w których były dane.
    /// </summary>
    Task<IReadOnlyList<ResponseTimeBucket>> GetResponseTimeBucketsAsync(
        Guid monitorId,
        DateTimeOffset from,
        TimeSpan bucketSize,
        ResponseTimeSource source,
        CancellationToken ct = default);
}

public enum RollupGranularity
{
    Hourly,
    Daily
}

/// <summary>Skąd liczyć czasy odpowiedzi — surowe checki są dokładniejsze, ale trzymane tylko 30 dni.</summary>
public enum ResponseTimeSource
{
    RawChecks,
    HourlyRollup
}

public sealed record CheckTotals(long TotalChecks, long UpChecks)
{
    public static readonly CheckTotals Empty = new(0, 0);

    public double? UptimeRatio => TotalChecks == 0 ? null : (double)UpChecks / TotalChecks;
}

public sealed record ResponseTimeBucket(
    DateTimeOffset Bucket,
    long Count,
    long SumMs,
    int MinMs,
    int MaxMs);
