using FluentValidation;

namespace US.Application.Monitors.Commands.CreateMonitor;

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
    }
}