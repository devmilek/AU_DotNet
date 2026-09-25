using FluentValidation;
using FluentValidation.Results;
using US.Domain.Entities;

namespace US.Application.MaintenanceWindows;

internal static class MaintenanceSchedule
{
    public static readonly TimeSpan Horizon = TimeSpan.FromDays(90);

    public const int MaxOccurrencesInHorizon = 500;

    public static IReadOnlyList<MaintenanceOccurrence> Sync(
        MaintenanceWindow window,
        IRecurrenceExpander expander,
        DateTimeOffset now)
    {
        var starts = Plan(window, expander, now);

        try
        {
            return window.SyncOccurrences(starts, now);
        }
        catch (ArgumentException ex)
        {
            throw Invalid("Schedule", ex.Message);
        }
    }

    private static IReadOnlyList<DateTime> Plan(MaintenanceWindow window, IRecurrenceExpander expander, DateTimeOffset now)
    {
        if (!window.IsRecurring)
        {
            if (window.ToUtc(window.StartsAtLocal) + window.Duration <= now)
                throw Invalid("Schedule.StartsAtLocal", "Jednorazowe okno serwisowe musi kończyć się w przyszłości.");

            return [window.StartsAtLocal];
        }

        IReadOnlyList<DateTime> starts;
        try
        {
            starts = expander.Expand(
                window.TimeZoneId,
                window.StartsAtLocal,
                window.RecurrenceRule!,
                window.RecurrenceEndLocal,
                now - window.Duration,
                now + Horizon,
                MaxOccurrencesInHorizon + 1);
        }
        catch (ArgumentException ex)
        {
            throw Invalid("Schedule.RecurrenceRule", $"Niepoprawna reguła RRULE: {ex.Message}");
        }

        if (starts.Count > MaxOccurrencesInHorizon)
            throw Invalid(
                "Schedule.RecurrenceRule",
                $"Reguła daje ponad {MaxOccurrencesInHorizon} wystąpień w ciągu {Horizon.TotalDays:0} dni — jest zbyt częsta.");

        return starts;
    }

    private static ValidationException Invalid(string property, string message) =>
        new([new ValidationFailure(property, message)]);
}
