using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using AU.Application.Notifications;

namespace AU.Infrastructure.Notifications;

/// <summary>
/// Renderuje maile ze szablonów Scriban osadzonych w assembly (Notifications/Templates).
/// Szablon maila to sama treść — ramkę (logo, karta, stopka, style) daje <c>_layout.html</c>.
/// Treść ustawia zmienne <c>title</c>, <c>preheader</c> i opcjonalnie <c>footer_note</c>, które layout wykorzystuje.
/// Właściwości modelu są dostępne w snake_case (<c>MonitorName</c> → <c>monitor_name</c>).
/// </summary>
public sealed class ScribanEmailTemplateRenderer(IOptions<FrontendOptions> frontendOptions) : IEmailTemplateRenderer
{
    private const string LayoutTemplate = "_layout";

    private static readonly EmbeddedTemplateLoader Loader = new();
    private static readonly Lazy<string> Styles = new(() => EmbeddedTemplateLoader.ReadResource("_styles.css"));
    private static readonly Lazy<string> MediaStyles = new(() => EmbeddedTemplateLoader.ReadResource("_styles-media.css"));

    private readonly ConcurrentDictionary<string, Template> _compiledCache = new();

    public string Render<TModel>(string templateName, TModel model)
    {
        var globals = new ScriptObject
        {
            ["title"] = "",
            ["preheader"] = "",
            ["footer_note"] = null,
            ["app_url"] = frontendOptions.Value.FrontendUrl,
            ["year"] = DateTime.UtcNow.Year
        };
        globals.Import(model, renamer: StandardMemberRenamer.Default);

        var context = new TemplateContext
        {
            TemplateLoader = Loader,
            // literówka w nazwie zmiennej ma rzucić wyjątek, a nie wyrenderować pusty tekst
            StrictVariables = true
        };
        context.PushGlobal(globals);

        // treść renderujemy w tym samym kontekście, żeby ustawione w niej zmienne (title, preheader…) trafiły do layoutu
        globals["content"] = GetTemplate(templateName).Render(context);
        globals["styles"] = Styles.Value;
        globals["media_styles"] = MediaStyles.Value;

        return GetTemplate(LayoutTemplate).Render(context);
    }

    private Template GetTemplate(string templateName) =>
        _compiledCache.GetOrAdd(templateName, name =>
        {
            var template = Template.Parse(EmbeddedTemplateLoader.ReadResource($"{name}.html"), $"{name}.html");

            if (template.HasErrors)
            {
                var errors = string.Join(Environment.NewLine, template.Messages.Select(x => x.Message));
                throw new InvalidOperationException(
                    $"Błąd parsowania szablonu '{name}':{Environment.NewLine}{errors}");
            }

            return template;
        });

    /// <summary>Ładuje szablony (także te z <c>include</c>) z zasobów osadzonych.</summary>
    private sealed class EmbeddedTemplateLoader : ITemplateLoader
    {
        private const string ResourcePrefix = "AU.Infrastructure.Notifications.Templates.";
        private static readonly Assembly Assembly = typeof(ScribanEmailTemplateRenderer).Assembly;

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) =>
            $"{templateName}.html";

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
            ReadResource(templatePath);

        public ValueTask<string?> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
            ValueTask.FromResult<string?>(ReadResource(templatePath));

        public static string ReadResource(string fileName)
        {
            var resourceName = ResourcePrefix + fileName;
            using var stream = Assembly.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Nie znaleziono szablonu: {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
