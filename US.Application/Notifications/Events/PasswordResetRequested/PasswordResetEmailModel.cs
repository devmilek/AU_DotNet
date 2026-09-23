namespace US.Application.Notifications.Events.PasswordResetRequested;

public record PasswordResetEmailModel(
    string DisplayName,
    string ResetUrl,
    int ExpiresInMinutes);
