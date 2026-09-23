using US.Application.Common;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Application.Monitors.Queries.GetAllMonitors;

public class GetAllMonitorsHandler
{
    public async Task<PagedResult<Monitor>> Handle(
        GetAllMonitorsQuery query,
        IMonitorRepository monitorRepository,
        CancellationToken cancellationToken)
    {
        return await monitorRepository.GetPagedAsync(query, cancellationToken);
    }
}
