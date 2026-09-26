using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Application.Checks.Commands.CheckMonitor;

public sealed record CheckMonitorCommand(Guid MonitorId);