using AU.Domain.Enums;

namespace AU.Domain.Entities;

public class IncidentNotification
{
    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }

    public NotificationType Type { get; private set; }
    public ChannelType Channel { get; private set; }
    public NotificationStatus Status { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private IncidentNotification() { }

    public static IncidentNotification CreatePending(
        Guid incidentId,
        NotificationType type,
        ChannelType channel)
    {
        if (incidentId == Guid.Empty)
            throw new ArgumentException("IncidentId nie może być pusty.", nameof(incidentId));

        return new IncidentNotification
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            Type = type,
            Channel = channel,
            Status = NotificationStatus.Pending
        };
    }

    public void MarkSent(DateTimeOffset sentAt)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Tylko oczekujące powiadomienie może zostać oznaczone jako wysłane.");

        Status = NotificationStatus.Sent;
        SentAt = sentAt;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Nieudane powiadomienie wymaga error_message.", nameof(errorMessage));

        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Tylko oczekujące powiadomienie może zostać oznaczone jako nieudane.");

        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
    }
}