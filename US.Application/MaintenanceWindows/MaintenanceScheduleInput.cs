using FluentValidation;
using US.Domain.Entities;

namespace US.Application.MaintenanceWindows;

public sealed record MaintenanceScheduleInput(
    string TimeZoneId,
    DateTime StartsAtLocal,
    int DurationMinutes,
    string? RecurrenceRule = null,
    DateTime? RecurrenceEndLocal = null);

public sealed class MaintenanceScheduleInputValidator : AbstractValidator<MaintenanceScheduleInput>
{
    private const string WallClockMessage =
        "Podaj czas ścienny bez strefy i offsetu, np. 2026-10-01T02:00:00 — strefę określa TimeZoneId.";

    public MaintenanceScheduleInputValidator()
    {
        RuleFor(x => x.TimeZoneId)
            .NotEmpty()
            .Must(MaintenanceWindow.IsKnownTimeZone)
            .WithMessage(x => $"Nieznana strefa czasowa IANA: '{x.TimeZoneId}'.");

        RuleFor(x => x.StartsAtLocal)
            .Must(d => d.Kind == DateTimeKind.Unspecified)
            .WithMessage(WallClockMessage);

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(MaintenanceWindow.MinDurationMinutes, MaintenanceWindow.MaxDurationMinutes);

        RuleFor(x => x.RecurrenceRule)
            .MaximumLength(MaintenanceWindow.MaxRecurrenceRuleLength)
            .Must(rule => rule!.Contains("FREQ=", StringComparison.OrdinalIgnoreCase))
            .WithMessage("RRULE musi zawierać FREQ.")
            .Must(rule => !rule!.Contains("DTSTART", StringComparison.OrdinalIgnoreCase))
            .WithMessage("RRULE nie może zawierać DTSTART — początek to StartsAtLocal.")
            .When(x => !string.IsNullOrWhiteSpace(x.RecurrenceRule));

        When(x => x.RecurrenceEndLocal is not null, () =>
        {
            RuleFor(x => x.RecurrenceEndLocal)
                .Must((x, _) => !string.IsNullOrWhiteSpace(x.RecurrenceRule))
                .WithMessage("Koniec serii ma sens tylko dla okna cyklicznego.")
                .Must(d => d!.Value.Kind == DateTimeKind.Unspecified)
                .WithMessage(WallClockMessage)
                .Must((x, d) => d >= x.StartsAtLocal)
                .WithMessage("Koniec serii nie może być wcześniejszy niż pierwsze wystąpienie.")
                .Must((x, _) => x.RecurrenceRule is null
                                || (!x.RecurrenceRule.Contains("UNTIL=", StringComparison.OrdinalIgnoreCase)
                                    && !x.RecurrenceRule.Contains("COUNT=", StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Podaj koniec serii albo UNTIL/COUNT w RRULE, nie oba naraz.");
        });
    }
}
