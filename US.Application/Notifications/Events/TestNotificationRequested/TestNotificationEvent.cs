namespace US.Application.Notifications.Events.TestNotificationRequested;

public record TestNotificationEvent(Guid MonitorId, DateTimeOffset RequestedAt);