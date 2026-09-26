
namespace AU.Application.Incidents.Events.MonitorCheckCompleted;

public record MonitorCheckCompletedEvent(Guid MonitorId, bool IsUp, DateTimeOffset CheckedAt, string? ErrorMessage = null);