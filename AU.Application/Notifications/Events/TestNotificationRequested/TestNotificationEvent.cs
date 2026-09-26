namespace AU.Application.Notifications.Events.TestNotificationRequested;

public record TestNotificationEvent(Guid MonitorId, DateTimeOffset RequestedAt);