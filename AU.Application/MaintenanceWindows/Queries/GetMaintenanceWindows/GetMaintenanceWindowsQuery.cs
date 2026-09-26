namespace AU.Application.MaintenanceWindows.Queries.GetMaintenanceWindows;

public sealed record GetMaintenanceWindowsQuery(Guid OrganizationId);

public sealed class GetMaintenanceWindowsHandler
{
    public Task<IReadOnlyList<MaintenanceWindowListRow>> Handle(
        GetMaintenanceWindowsQuery query,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        return windowRepository.ListAsync(query.OrganizationId, timeProvider.GetUtcNow(), cancellationToken);
    }
}
