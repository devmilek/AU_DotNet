using AU.Application.MaintenanceWindows;

namespace AU.Api.Controllers.MaintenanceWindows.Requests;

public sealed record CreateMaintenanceWindowRequest(
    string Name,
    string? Description,
    MaintenanceScheduleInput Schedule,
    IReadOnlyList<Guid> MonitorIds,
    bool SuppressNotifications = true,
    bool ExcludeFromSla = true);

public sealed record RenameMaintenanceWindowRequest(
    string Name,
    string? Description,
    bool ApplyToPastOccurrences = false);

public sealed record UpdateMaintenanceWindowPolicyRequest(bool SuppressNotifications, bool ExcludeFromSla);

public sealed record SetMaintenanceWindowMonitorsRequest(IReadOnlyList<Guid> MonitorIds);

public sealed record UpdateOccurrenceContentRequest(string Name, string? Description);
