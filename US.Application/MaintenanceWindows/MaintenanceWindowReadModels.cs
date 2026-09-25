using US.Domain.Entities;

namespace US.Application.MaintenanceWindows;

public sealed record OccurrenceSlot(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);

public sealed record MaintenanceWindowListRow(MaintenanceWindow Window, int MonitorCount, OccurrenceSlot? NextOccurrence);

public sealed record MaintenanceOccurrenceRow(
    Guid Id,
    Guid MaintenanceWindowId,
    string Name,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
