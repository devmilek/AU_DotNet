namespace AU.Api.Controllers.Incidents.Requests;

public sealed record UpdateIncidentRequest(string? Name, string? Cause);
