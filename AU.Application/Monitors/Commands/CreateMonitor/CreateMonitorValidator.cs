using FluentValidation;
using AU.Domain.Enums;
using AU.Domain.ValueObjects.Checks;

namespace AU.Application.Monitors.Commands.CreateMonitor;

public sealed class CreateMonitorValidator: AbstractValidator<CreateMonitorCommand>
{
    public CreateMonitorValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Target)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.IntervalSeconds)
            .GreaterThan(0);

        RuleFor(x => x.TimeoutMs)
            .GreaterThan(0);

        RuleFor(x => x.AlertThreshold)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.RecoveryThreshold)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Http)
            .Null()
            .When(x => x.Type != MonitorType.Http)
            .WithMessage("Ustawienia HTTP są dozwolone tylko dla monitora typu Http.");

        When(x => x.Type == MonitorType.Http, () =>
        {
            RuleFor(x => x.Target)
                .Must(target => Uri.TryCreate(target, UriKind.Absolute, out var uri)
                                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Target monitora HTTP musi być adresem http:// lub https://.");

            RuleFor(x => x.Http!)
                .SetValidator(new HttpCheckSettingsValidator())
                .When(x => x.Http is not null);
        });

        When(x => x.Type == MonitorType.Tcp, () =>
        {
            RuleFor(x => x.Target)
                .Must(target => TcpEndpoint.TryParse(target, out _))
                .WithMessage("Target monitora TCP musi mieć postać host:port, np. db.example.com:5432.");
        });

        RuleFor(x => x.Type)
            .Must(type => type is MonitorType.Http or MonitorType.Tcp)
            .WithMessage("Obsługiwane są na razie monitory HTTP i TCP.");
    }
}

public sealed class HttpCheckSettingsValidator : AbstractValidator<HttpCheckSettings>
{
    public HttpCheckSettingsValidator()
    {
        RuleFor(x => x.Method)
            .IsInEnum();

        RuleFor(x => x.AcceptedStatusCodes)
            .Must(ranges => ranges!.Count <= 20)
            .WithMessage("Maksymalnie 20 zakresów status code.")
            .When(x => x.AcceptedStatusCodes is not null);

        RuleForEach(x => x.AcceptedStatusCodes)
            .ChildRules(range =>
            {
                range.RuleFor(r => r.From).InclusiveBetween(100, 599);
                range.RuleFor(r => r.To).InclusiveBetween(100, 599);
                range.RuleFor(r => r)
                    .Must(r => r.From <= r.To)
                    .WithName("AcceptedStatusCodes")
                    .WithMessage("Początek zakresu nie może być większy niż koniec.");
            });

        RuleFor(x => x.Auth!)
            .SetValidator(new HttpAuthSettingsValidator())
            .When(x => x.Auth is not null);
    }
}

public sealed class HttpAuthSettingsValidator : AbstractValidator<HttpAuthSettings>
{
    public HttpAuthSettingsValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum();

        When(x => x.Type == HttpAuthType.Basic, () =>
        {
            // RFC 7617 — dwukropek w nazwie użytkownika rozbiłby nagłówek Basic
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(256)
                .Must(username => username is null || !username.Contains(':'))
                .WithMessage("Nazwa użytkownika nie może zawierać dwukropka.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MaximumLength(1024);
        });

        When(x => x.Type == HttpAuthType.Bearer, () =>
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .MaximumLength(4096);
        });
    }
}
