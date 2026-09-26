using AU.Application.Common;

namespace AU.Application.Incidents;

public interface IIncidentReader
{
    Task<PagedResult<IncidentRow>> GetPagedAsync(
        Guid organizationId,
        IncidentStatusFilter status,
        Guid? monitorId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IncidentRow?> GetAsync(Guid organizationId, Guid incidentId, CancellationToken ct = default);
}
