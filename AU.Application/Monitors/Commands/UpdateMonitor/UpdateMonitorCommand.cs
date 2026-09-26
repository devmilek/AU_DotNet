using FluentValidation;
using FluentValidation.Results;
using AU.Application.Abstractions;
using AU.Application.Exceptions;
using AU.Application.Monitors.Commands.CreateMonitor;
using AU.Domain.Enums;
using AU.Domain.ValueObjects.Checks;

namespace AU.Application.Monitors.Commands.UpdateMonitor;

public sealed record UpdateMonitorCommand(
    Guid OrganizationId,
    Guid MonitorId,
    string Name,
    string Target,
    int IntervalSeconds,
    int TimeoutMs,
    int AlertThreshold,
    int RecoveryThreshold,
    HttpCheckSettings? Http = null);

public sealed class UpdateMonitorValidator : AbstractValidator<UpdateMonitorCommand>
{
    public UpdateMonitorValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.MonitorId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Target)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.IntervalSeconds)
            .GreaterThanOrEqualTo(5);

        RuleFor(x => x.TimeoutMs)
            .GreaterThanOrEqualTo(100)
            .LessThan(x => x.IntervalSeconds * 1000)
            .WithMessage("Timeout musi być krótszy niż interwał checków.");

        RuleFor(x => x.AlertThreshold)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.RecoveryThreshold)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Http!)
            .SetValidator(new HttpCheckSettingsValidator(secretsRequired: false))
            .When(x => x.Http is not null);
    }
}

public sealed class UpdateMonitorHandler
{
    public async Task Handle(
        UpdateMonitorCommand command,
        IMonitorRepository repository,
        ISecretProtector secretProtector,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitor = await repository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", command.MonitorId);

        if (command.Http is not null && monitor.Type != MonitorType.Http)
            throw new ValidationException([new ValidationFailure(nameof(command.Http), "Ustawienia HTTP są dozwolone tylko dla monitora typu Http.")]);

        if (monitor.Type == MonitorType.Http
            && !(Uri.TryCreate(command.Target, UriKind.Absolute, out var uri)
                 && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            throw new ValidationException([new ValidationFailure(nameof(command.Target), "Target monitora HTTP musi być adresem http:// lub https://.")]);

        if (monitor.Type == MonitorType.Tcp && !TcpEndpoint.TryParse(command.Target, out _))
            throw new ValidationException([new ValidationFailure(nameof(command.Target), "Target monitora TCP musi mieć postać host:port, np. db.example.com:5432.")]);

        monitor.Rename(command.Name);
        monitor.UpdateTarget(command.Target);
        monitor.UpdateSchedule(command.IntervalSeconds, command.TimeoutMs);
        monitor.UpdateThresholds(command.AlertThreshold, command.RecoveryThreshold);

        if (command.Http is { } http)
            monitor.UpdateConfig(HttpCheckSettingsMapper.ToConfig(http, secretProtector, monitor.Config as HttpCheckConfig));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
