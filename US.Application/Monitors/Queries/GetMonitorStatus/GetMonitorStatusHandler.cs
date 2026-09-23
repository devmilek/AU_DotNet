using US.Application.Checks.Statistics;
using US.Application.Exceptions;

namespace US.Application.Monitors.Queries.GetMonitorStatus;

public sealed class GetMonitorStatusHandler
{
    public async Task<MonitorStatusResult> Handle(
        GetMonitorStatusQuery query,
        IMonitorRepository monitorRepository,
        IIncidentRepository incidentRepository,
        IMonitorStatisticsReader statistics,
        CancellationToken cancellationToken)
    {
        var monitor = await monitorRepository.GetAsync(query.OrganizationId, query.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", query.MonitorId);

        var latest = await statistics.GetLatestCheckAsync(monitor.Id, cancellationToken);
        var lastCheck = latest is null
            ? null
            : new LastCheckResult(latest.CheckedAt, latest.Status, latest.ResponseTimeMs, latest.StatusCode, latest.ErrorMessage);

        if (!monitor.IsActive)
            return new MonitorStatusResult(MonitorStatus.Paused, null, lastCheck);

        // "down" = otwarty incydent, spójnie z sortowaniem listy i powiadomieniami
        var openIncident = await incidentRepository.GetOpenForMonitorAsync(monitor.Id);
        if (openIncident is not null)
            return new MonitorStatusResult(MonitorStatus.Down, openIncident.StartedAt, lastCheck);

        if (lastCheck is null)
            return new MonitorStatusResult(MonitorStatus.Pending, monitor.CreatedAt, null);

        var lastResolved = await incidentRepository.GetLastResolvedForMonitorAsync(monitor.Id);
        var upSince = lastResolved?.ResolvedAt is { } resolvedAt && resolvedAt > monitor.CreatedAt
            ? resolvedAt
            : monitor.CreatedAt;

        return new MonitorStatusResult(MonitorStatus.Up, upSince, lastCheck);
    }
}
