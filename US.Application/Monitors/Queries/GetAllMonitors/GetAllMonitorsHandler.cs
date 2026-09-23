using US.Application.Checks.Statistics;
using US.Application.Common;

namespace US.Application.Monitors.Queries.GetAllMonitors;

public class GetAllMonitorsHandler
{
    public async Task<PagedResult<MonitorListItem>> Handle(
        GetAllMonitorsQuery query,
        IMonitorRepository monitorRepository,
        IMonitorStatisticsReader statistics,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var page = await monitorRepository.GetPagedAsync(query, cancellationToken);

        // jedno zapytanie o historię dla całej strony zamiast N osobnych
        var windowStart = HourlyHistory.WindowStart(timeProvider.GetUtcNow());
        var rollups = await statistics.GetHourlyAsync(
            page.Items.Select(m => m.Id).ToList(), windowStart, cancellationToken);
        var index = HourlyHistory.Index(rollups);

        var items = page.Items
            .Select(monitor => new MonitorListItem(monitor, HourlyHistory.Build(monitor, windowStart, index)))
            .ToList();

        return new PagedResult<MonitorListItem>(items, page.Page, page.PageSize, page.TotalCount);
    }
}
