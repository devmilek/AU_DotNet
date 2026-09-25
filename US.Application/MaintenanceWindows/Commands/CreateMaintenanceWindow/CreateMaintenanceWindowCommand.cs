namespace US.Application.MaintenanceWindows.Commands.CreateMaintenanceWindow;

public sealed record CreateMaintenanceWindowCommand(
    Guid OrganizationId,
    string Name,
    string? Description,
    MaintenanceScheduleInput Schedule,
    IReadOnlyList<Guid> MonitorIds,
    bool SuppressNotifications = true,
    bool ExcludeFromSla = true);
