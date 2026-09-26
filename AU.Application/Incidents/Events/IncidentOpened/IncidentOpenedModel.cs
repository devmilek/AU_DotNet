namespace AU.Application.Incidents.Events.IncidentOpened;

public record IncidentOpenedModel(
    string MonitorName,
    string MonitorTarget,
    string StartedAt,
    int FailedChecksCount,
    string MonitorUrl);
