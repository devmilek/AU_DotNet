using System.Collections.Concurrent;
using Scriban;
using US.Application.Notifications;

namespace US.Infrastructure.Notifications;

public class ScribanEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly ConcurrentDictionary<string, Template> _compiledCache = new();

    public string Render<TModel>(string templateName, TModel model)
    {
        var template = _compiledCache.GetOrAdd(templateName, name =>
        {
            var source = LoadTemplateSource(name);
            var template = Template.Parse(source);

            if (template.HasErrors)
            {
                var errors = string.Join(
                    Environment.NewLine,
                    template.Messages.Select(x => x.Message));

                throw new InvalidOperationException(
                    $"Błąd parsowania szablonu '{name}':{Environment.NewLine}{errors}");
            }

            return template;
        });
        
        return template.Render(model);
    }
    
    private static string LoadTemplateSource(string templateName)
    {
        var assembly = typeof(ScribanEmailTemplateRenderer).Assembly;
        var resourceName =
            $"US.Infrastructure.Notifications.Templates.{templateName}.html";
        var resources = assembly.GetManifestResourceNames();

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new FileNotFoundException(
                               $"Nie znaleziono szablonu: {resourceName}");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}