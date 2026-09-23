
using US.Domain.Entities;

public interface IIncidentRepository
{
    Task<Incident?> GetOpenForMonitorAsync(Guid monitorId);
    void Add(Incident incident);
}