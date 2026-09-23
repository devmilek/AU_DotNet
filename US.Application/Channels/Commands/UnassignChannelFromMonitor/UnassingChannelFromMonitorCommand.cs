namespace US.Application.Channels.Commands.UnassignChannelFromMonitor;

public record UnassingChannelFromMonitorCommand(Guid OrganizationId, Guid MonitorId, Guid ChannelId);
