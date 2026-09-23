namespace US.Application.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message);
}