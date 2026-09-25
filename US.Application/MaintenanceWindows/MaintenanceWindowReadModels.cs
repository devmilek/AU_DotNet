using US.Application.Channels;
using US.Domain.Entities;

namespace US.Application.MaintenanceWindows;

public sealed record OccurrenceSlot(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);

public sealed record WindowMonitorName(Guid Id, string Name);

public sealed record MaintenanceWindowListRow(
    MaintenanceWindow Window,
    IReadOnlyList<WindowMonitorName> Monitors,
    OccurrenceSlot? NextOccurrence);

public sealed record MaintenanceOccurrenceRow(
    Guid Id,
    Guid MaintenanceWindowId,
    string Name,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public sealed record MaintenanceWindowDetails(
    MaintenanceWindow Window,
    IReadOnlyList<MonitorSummary> Monitors,
    OccurrenceSlot? NextOccurrence);

public sealed record WindowOccurrenceRow(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    DateTimeOffset ScheduledStartUtc,
    MaintenanceOccurrenceStatus Status,
    bool FollowsWindow,
    bool DiffersFromWindow);
