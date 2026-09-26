using AU.Application.Common;
using AU.Application.Monitors.Queries.GetAllMonitors;
using AU.Application.Monitors.Queries.GetMonitorStatuses;

namespace AU.Application.Monitors;
using Monitor = AU.Domain.Entities.Monitor;

public interface IMonitorRepository
{
    Task AddAsync(Monitor monitor, CancellationToken ct = default);

    // ścieżka API — zawsze z organizacją
    Task<Monitor?> GetAsync(Guid organizationId, Guid monitorId, CancellationToken ct = default);
    Task<PagedResult<Monitor>> GetPagedAsync(GetAllMonitorsQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<MonitorStatusRow>> ListStatusRowsAsync(Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> GetExistingIdsAsync(Guid organizationId, IReadOnlyCollection<Guid> monitorIds, CancellationToken ct = default);

    // ścieżka workerów — bez organizacji, świadomie
    Task<Monitor?> GetForCheckAsync(Guid monitorId, CancellationToken ct = default);

    void Remove(Monitor monitor);
}
