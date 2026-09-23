using Monitor = US.Domain.Entities.Monitor;

namespace US.Application.Monitors.Queries.GetAllMonitors;

public class GetAllMonitorsHandler
{
    public async Task<IReadOnlyList<Monitor>> Handle(
        GetAllMonitorsQuery query,
        IMonitorRepository monitorRepository,
        CancellationToken cancellationToken)
    {
        return await monitorRepository.GetAllAsync(
            query.OrganizationId,
            cancellationToken);
    }
}
