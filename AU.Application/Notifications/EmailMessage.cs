namespace AU.Application.Notifications;

public record EmailMessage(IReadOnlyList<string> To, string Subject, string HtmlBody);