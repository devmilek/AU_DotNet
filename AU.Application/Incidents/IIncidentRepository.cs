using AU.Domain.Entities;

public interface IIncidentRepository
{
    Task<Incident?> GetOpenForMonitorAsync(Guid monitorId);
    Task<Incident?> GetLastResolvedForMonitorAsync(Guid monitorId);
    Task<Incident?> GetAsync(Guid organizationId, Guid incidentId, CancellationToken ct = default);
    void Add(Incident incident);
    void Remove(Incident incident);
}
