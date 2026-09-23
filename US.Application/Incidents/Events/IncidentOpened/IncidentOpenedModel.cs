namespace US.Application.Incidents.Events.IncidentOpened;

public record IncidentOpenedModel(
    string MonitorName,
    string StartedAt,
    int FailedChecksCount,
    string MonitorUrl);