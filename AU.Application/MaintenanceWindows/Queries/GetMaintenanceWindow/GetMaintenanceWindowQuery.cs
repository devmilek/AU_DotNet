using AU.Application.Exceptions;

namespace AU.Application.MaintenanceWindows.Queries.GetMaintenanceWindow;

public sealed record GetMaintenanceWindowQuery(Guid OrganizationId, Guid WindowId);

public sealed class GetMaintenanceWindowHandler
{
    public async Task<MaintenanceWindowDetails> Handle(
        GetMaintenanceWindowQuery query,
        IMaintenanceWindowRepository windowRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        return await windowRepository.GetDetailsAsync(
                   query.OrganizationId, query.WindowId, timeProvider.GetUtcNow(), cancellationToken)
               ?? throw new NotFoundException("Okno serwisowe", query.WindowId);
    }
}
