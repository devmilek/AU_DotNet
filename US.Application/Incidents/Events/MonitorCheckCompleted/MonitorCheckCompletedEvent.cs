
namespace US.Application.Incidents.Events.MonitorCheckCompleted;

public record MonitorCheckCompletedEvent(Guid MonitorId, bool IsUp, DateTimeOffset CheckedAt);