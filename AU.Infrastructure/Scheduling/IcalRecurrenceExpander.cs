using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using AU.Application.MaintenanceWindows;

namespace AU.Infrastructure.Scheduling;

public sealed class IcalRecurrenceExpander : IRecurrenceExpander
{
    public IReadOnlyList<DateTime> Expand(
        string timeZoneId,
        DateTime startsAtLocal,
        string recurrenceRule,
        DateTime? recurrenceEndLocal,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int limit)
    {
        RecurrencePattern pattern;
        try
        {
            pattern = new RecurrencePattern(recurrenceRule);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new ArgumentException(ex.Message, nameof(recurrenceRule), ex);
        }

        var calendarEvent = new CalendarEvent
        {
            DtStart = new CalDateTime(DateTime.SpecifyKind(startsAtLocal, DateTimeKind.Unspecified), timeZoneId),
            RecurrenceRule = pattern
        };

        return calendarEvent
            .GetOccurrences(new CalDateTime(fromUtc.UtcDateTime, "UTC"))
            .TakeWhileBefore(new CalDateTime(toUtc.UtcDateTime, "UTC"))
            .Select(o => DateTime.SpecifyKind(o.Period.StartTime.Value, DateTimeKind.Unspecified))
            .TakeWhile(start => recurrenceEndLocal is not { } end || start <= end)
            .Take(limit)
            .ToList();
    }
}
