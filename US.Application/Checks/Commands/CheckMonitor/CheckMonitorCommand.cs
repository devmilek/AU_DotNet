using Monitor = US.Domain.Entities.Monitor;

namespace US.Application.Checks.Commands.CheckMonitor;

public sealed record CheckMonitorCommand(Guid MonitorId);