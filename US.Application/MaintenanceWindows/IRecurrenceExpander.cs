namespace US.Application.MaintenanceWindows;

public interface IRecurrenceExpander
{
    IReadOnlyList<DateTime> Expand(
        string timeZoneId,
        DateTime startsAtLocal,
        string recurrenceRule,
        DateTime? recurrenceEndLocal,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int limit);
}
