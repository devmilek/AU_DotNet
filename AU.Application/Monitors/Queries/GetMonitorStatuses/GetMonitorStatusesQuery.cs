using AU.Application.Monitors.Queries.GetMonitorStatus;

namespace AU.Application.Monitors.Queries.GetMonitorStatuses;

public sealed record GetMonitorStatusesQuery(Guid OrganizationId);

public sealed record MonitorStatusRow(Guid Id, string Name, bool IsActive, bool HasOpenIncident, bool HasChecks);

public sealed record MonitorStatusSummary(Guid Id, string Name, MonitorStatus Status);

public sealed class GetMonitorStatusesHandler
{
    public async Task<IReadOnlyList<MonitorStatusSummary>> Handle(
        GetMonitorStatusesQuery query,
        IMonitorRepository monitorRepository,
        CancellationToken cancellationToken)
    {
        var rows = await monitorRepository.ListStatusRowsAsync(query.OrganizationId, cancellationToken);

        return rows
            .Select(row => new MonitorStatusSummary(row.Id, row.Name, Resolve(row)))
            .ToList();
    }

    private static MonitorStatus Resolve(MonitorStatusRow row) => row switch
    {
        { IsActive: false } => MonitorStatus.Paused,
        { HasOpenIncident: true } => MonitorStatus.Down,
        { HasChecks: false } => MonitorStatus.Pending,
        _ => MonitorStatus.Up
    };
}
