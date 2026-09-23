using US.Application.Common;
using US.Application.Monitors.Queries.GetAllMonitors;

namespace US.Application.Monitors;
using Monitor = US.Domain.Entities.Monitor;

public interface IMonitorRepository
{
    Task AddAsync(Monitor monitor, CancellationToken ct = default);

    // ścieżka API — zawsze z organizacją
    Task<Monitor?> GetAsync(Guid organizationId, Guid monitorId, CancellationToken ct = default);
    Task<PagedResult<Monitor>> GetPagedAsync(GetAllMonitorsQuery query, CancellationToken ct = default);

    // ścieżka workerów — bez organizacji, świadomie
    Task<Monitor?> GetForCheckAsync(Guid monitorId, CancellationToken ct = default);

    void Remove(Monitor monitor);
}
