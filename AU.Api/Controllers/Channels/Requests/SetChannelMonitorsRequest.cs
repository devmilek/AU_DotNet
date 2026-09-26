namespace AU.Api.Controllers.Channels.Requests;

/// <param name="MonitorIds">Pełna lista monitorów przypiętych do kanału — pozostałe zostaną odpięte.</param>
public sealed record SetChannelMonitorsRequest(IReadOnlyList<Guid> MonitorIds);
