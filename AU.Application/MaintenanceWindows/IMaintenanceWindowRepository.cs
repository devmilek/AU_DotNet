using AU.Domain.Entities;

namespace AU.Application.MaintenanceWindows;

public interface IMaintenanceWindowRepository
{
    void Add(MaintenanceWindow window);

    Task<MaintenanceWindow?> GetAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset now,
        bool includePastOccurrences = false,
        CancellationToken ct = default);

    Task<MaintenanceWindowDetails?> GetDetailsAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset now,
        CancellationToken ct = default);

    Task<IReadOnlyList<WindowOccurrenceRow>?> ListWindowOccurrencesAsync(
        Guid organizationId,
        Guid windowId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default);

    Task<MaintenanceWindow?> GetForOccurrenceAsync(
        Guid organizationId,
        Guid windowId,
        Guid occurrenceId,
        DateTimeOffset now,
        CancellationToken ct = default);

    Task<IReadOnlyList<MaintenanceWindowListRow>> ListAsync(
        Guid organizationId,
        DateTimeOffset now,
        CancellationToken ct = default);

    Task<IReadOnlyList<MaintenanceOccurrenceRow>> ListOccurrencesAsync(
        Guid organizationId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetRecurringIdsAsync(CancellationToken ct = default);

    Task<MaintenanceWindow?> GetForSchedulingAsync(Guid windowId, DateTimeOffset now, CancellationToken ct = default);

    void RemoveOccurrences(IEnumerable<MaintenanceOccurrence> occurrences);
}
