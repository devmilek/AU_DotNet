namespace AU.Application.Notifications.Events.PasswordResetRequested;

public record PasswordResetEmailModel(
    string DisplayName,
    string ResetUrl,
    int ExpiresInMinutes);
