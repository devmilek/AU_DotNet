using US.Domain.Entities;

namespace US.Application.MaintenanceWindows;

public interface IMaintenanceWindowRepository
{
    void Add(MaintenanceWindow window);

    Task<MaintenanceWindow?> GetAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset now,
        bool includePastOccurrences = false,
        CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetRecurringIdsAsync(CancellationToken ct = default);

    Task<MaintenanceWindow?> GetForSchedulingAsync(Guid windowId, DateTimeOffset now, CancellationToken ct = default);

    void RemoveOccurrences(IEnumerable<MaintenanceOccurrence> occurrences);
}
