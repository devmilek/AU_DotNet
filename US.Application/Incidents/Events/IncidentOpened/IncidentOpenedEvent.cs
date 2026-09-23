namespace US.Application.Incidents.Events.IncidentOpened;

public record IncidentOpenedEvent(Guid IncidentId, Guid MonitorId, DateTimeOffset OccurredAt);
