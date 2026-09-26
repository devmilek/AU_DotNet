namespace AU.Application.Notifications.Events.EmailConfirmationRequested;

public record EmailConfirmationEmailModel(
    string DisplayName,
    string ConfirmationUrl,
    int ExpiresInHours);