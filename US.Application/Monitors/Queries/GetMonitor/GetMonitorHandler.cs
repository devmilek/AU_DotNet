using Monitor = US.Domain.Entities.Monitor;

namespace US.Application.Monitors.Queries.GetMonitor;

public class GetMonitorHandler
{
    public async Task<Monitor?> Handle(
        GetMonitorQuery query,
        IMonitorRepository repository,
        CancellationToken cancellationToken)
    {
        return await repository.GetAsync(
            query.OrganizationId,
            query.MonitorId,
            cancellationToken);
    }
}
