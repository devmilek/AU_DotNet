namespace AU.Application.Notifications.Events.EmailConfirmationRequested;

public record EmailConfirmationRequestedEvent(string Email, string? DisplayName, string ConfirmationUrl);