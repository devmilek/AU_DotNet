using US.Application.MaintenanceWindows;

namespace US.Api.Controllers.MaintenanceWindows.Responses;

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
    int MonitorCount,
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
        row.MonitorCount,
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
