namespace US.Application.Notifications.Events.PasswordResetRequested;

public record PasswordResetRequestedEvent(string Email, string? DisplayName, string ResetUrl);