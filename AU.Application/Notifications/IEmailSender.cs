namespace AU.Application.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message);
}