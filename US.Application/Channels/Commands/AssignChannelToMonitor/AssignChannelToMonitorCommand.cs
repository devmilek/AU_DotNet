namespace US.Application.Channels.Commands.AssignChannelToMonitor;

public record AssignChannelToMonitorCommand(Guid OrganizationId, Guid MonitorId, Guid ChannelId);
