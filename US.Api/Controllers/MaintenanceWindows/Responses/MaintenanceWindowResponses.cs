using US.Application.MaintenanceWindows;
using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Api.Controllers.MaintenanceWindows.Responses;

public sealed record MonitorNameResponse(Guid Id, string Name);

public sealed record OccurrenceSlotResponse(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);

public sealed record MaintenanceWindowListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string TimeZoneId,
    DateTime StartsAtLocal,
    int DurationMinutes,
    string? RecurrenceRule,
    DateTime? RecurrenceEndLocal,
    bool SuppressNotifications,
    bool ExcludeFromSla,
    IReadOnlyList<MonitorNameResponse> Monitors,
    OccurrenceSlotResponse? NextOccurrence,
    DateTimeOffset CreatedAt)
{
    public static MaintenanceWindowListItemResponse From(MaintenanceWindowListRow row) => new(
        row.Window.Id,
        row.Window.Name,
        row.Window.Description,
        row.Window.TimeZoneId,
        row.Window.StartsAtLocal,
        row.Window.DurationMinutes,
        row.Window.RecurrenceRule,
        row.Window.RecurrenceEndLocal,
        row.Window.SuppressNotifications,
        row.Window.ExcludeFromSla,
        row.Monitors.Select(m => new MonitorNameResponse(m.Id, m.Name)).ToList(),
        row.NextOccurrence is { } next ? new OccurrenceSlotResponse(next.StartsAtUtc, next.EndsAtUtc) : null,
        row.Window.CreatedAt);
}

public sealed record MaintenanceOccurrenceResponse(
    Guid Id,
    Guid MaintenanceWindowId,
    string Name,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc)
{
    public static MaintenanceOccurrenceResponse From(MaintenanceOccurrenceRow row) => new(
        row.Id, row.MaintenanceWindowId, row.Name, row.StartsAtUtc, row.EndsAtUtc);
}

public sealed record MaintenanceWindowMonitorResponse(Guid Id, string Name, MonitorType Type, string Target, bool IsActive);

public sealed record MaintenanceWindowResponse(
    Guid Id,
    string Name,
    string? Description,
    string TimeZoneId,
    DateTime StartsAtLocal,
    int DurationMinutes,
    string? RecurrenceRule,
    DateTime? RecurrenceEndLocal,
    bool SuppressNotifications,
    bool ExcludeFromSla,
    IReadOnlyList<MaintenanceWindowMonitorResponse> Monitors,
    OccurrenceSlotResponse? NextOccurrence,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static MaintenanceWindowResponse From(MaintenanceWindowDetails details)
    {
        var window = details.Window;
        return new MaintenanceWindowResponse(
            window.Id,
            window.Name,
            window.Description,
            window.TimeZoneId,
            window.StartsAtLocal,
            window.DurationMinutes,
            window.RecurrenceRule,
            window.RecurrenceEndLocal,
            window.SuppressNotifications,
            window.ExcludeFromSla,
            details.Monitors
                .Select(m => new MaintenanceWindowMonitorResponse(m.Id, m.Name, m.Type, m.Target, m.IsActive))
                .ToList(),
            details.NextOccurrence is { } next ? new OccurrenceSlotResponse(next.StartsAtUtc, next.EndsAtUtc) : null,
            window.CreatedAt,
            window.UpdatedAt);
    }
}

public sealed record WindowOccurrenceResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    DateTimeOffset ScheduledStartUtc,
    MaintenanceOccurrenceStatus Status,
    bool FollowsWindow,
    bool DiffersFromWindow)
{
    public static WindowOccurrenceResponse From(WindowOccurrenceRow row) => new(
        row.Id,
        row.Name,
        row.Description,
        row.StartsAtUtc,
        row.EndsAtUtc,
        row.ScheduledStartUtc,
        row.Status,
        row.FollowsWindow,
        row.DiffersFromWindow);
}
