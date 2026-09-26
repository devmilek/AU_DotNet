namespace AU.Application.Incidents.Events.IncidentResolved;

public record IncidentResolvedModel(
    string MonitorName,
    string ResolvedAt,
    string Duration,
    string MonitorUrl
);