namespace AU.Application.Incidents.Events.IncidentResolved;

public record IncidentResolvedEvent(Guid IncidentId, Guid MonitorId, DateTimeOffset OccurredAt);