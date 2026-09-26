using AU.Application.Exceptions;
using AU.Application.Monitors;

namespace AU.Application.Channels;

internal static class MonitorAssignment
{
    public const int MaxMonitorsPerRequest = 500;

    /// <summary>
    /// Zwraca unikalne id monitorów, upewniając się, że wszystkie należą do organizacji.
    /// Obcy albo nieistniejący monitor = 404, żeby nie ujawniać zasobów innych organizacji.
    /// </summary>
    public static async Task<IReadOnlyCollection<Guid>> EnsureMonitorsExistAsync(
        IMonitorRepository monitorRepository,
        Guid organizationId,
        IReadOnlyList<Guid>? monitorIds,
        CancellationToken cancellationToken)
    {
        if (monitorIds is null || monitorIds.Count == 0) return [];

        var requested = monitorIds.ToHashSet();
        var existing = await monitorRepository.GetExistingIdsAsync(organizationId, requested, cancellationToken);

        var missing = requested.FirstOrDefault(id => !existing.Contains(id));
        if (missing != Guid.Empty)
            throw new NotFoundException("Monitor", missing);

        return requested;
    }
}
