using AU.Application.Checks.Statistics;
using AU.Application.Exceptions;

namespace AU.Application.Monitors.Queries.GetMonitorUptime;

public sealed class GetMonitorUptimeHandler
{
    public async Task<MonitorUptimeResult> Handle(
        GetMonitorUptimeQuery query,
        IMonitorRepository monitorRepository,
        IMonitorStatisticsReader statistics,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(query.OrganizationId, query.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", query.MonitorId);

        var now = timeProvider.GetUtcNow();

        // 24 h: te same wiersze co słupki — liczymy sumę w pamięci zamiast drugiego zapytania
        var hourlyStart = HourlyHistory.WindowStart(now);
        var hourly = await statistics.GetHourlyAsync([monitor.Id], hourlyStart, cancellationToken);
        var last24Hours = HourlyHistory.Build(monitor, hourlyStart, HourlyHistory.Index(hourly));
        var totals24Hours = new CheckTotals(
            hourly.Sum(h => h.TotalChecks),
            hourly.Sum(h => h.UpChecks));

        // okna wyrównane do kubełków agregatów; bieżący kubełek jest doliczany na żywo (real-time aggregate)
        var from7Days = TimeWindows.FloorToHour(now).AddHours(-(7 * 24 - 1));
        var from30Days = TimeWindows.FloorToDay(now).AddDays(-29);
        var from365Days = TimeWindows.FloorToDay(now).AddDays(-364);

        var totals7Days = await statistics.GetTotalsAsync(monitor.Id, from7Days, RollupGranularity.Hourly, cancellationToken);
        var totals30Days = await statistics.GetTotalsAsync(monitor.Id, from30Days, RollupGranularity.Daily, cancellationToken);
        var totals365Days = await statistics.GetTotalsAsync(monitor.Id, from365Days, RollupGranularity.Daily, cancellationToken);

        return new MonitorUptimeResult(
            [
                ToResult(UptimePeriod.Last24Hours, hourlyStart, totals24Hours),
                ToResult(UptimePeriod.Last7Days, from7Days, totals7Days),
                ToResult(UptimePeriod.Last30Days, from30Days, totals30Days),
                ToResult(UptimePeriod.Last365Days, from365Days, totals365Days)
            ],
            last24Hours);
    }

    private static UptimePeriodResult ToResult(UptimePeriod period, DateTimeOffset from, CheckTotals totals) =>
        new(period, from, totals.TotalChecks, totals.UpChecks, totals.UptimeRatio);
}
