namespace US.Application.Notifications.Events.PasswordResetRequested;

/// <param name="ExpiresInMinutes">Faktyczny czas życia tokenu resetu — do pokazania w mailu.</param>
public record PasswordResetRequestedEvent(string Email, string? DisplayName, string ResetUrl, int ExpiresInMinutes);
