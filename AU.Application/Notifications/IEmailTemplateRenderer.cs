namespace AU.Application.Notifications;

public interface IEmailTemplateRenderer
{
    string Render<TModel>(string templateName, TModel model);
}