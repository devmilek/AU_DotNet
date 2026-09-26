
using AU.Domain.Entities;

public interface IIncidentRepository
{
    Task<Incident?> GetOpenForMonitorAsync(Guid monitorId);
    Task<Incident?> GetLastResolvedForMonitorAsync(Guid monitorId);
    void Add(Incident incident);
}