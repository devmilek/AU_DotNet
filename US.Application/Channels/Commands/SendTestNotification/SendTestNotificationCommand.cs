namespace US.Application.Channels.Commands.SendTestNotification;

public record SendTestNotificationCommand(Guid OrganizationId, Guid MonitorId);
