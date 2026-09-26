using AU.Application.Checks.ReadModels;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Application.Checks.Statistics;

/// <summary>
/// Checki monitora w jednej godzinie. <see cref="ExpectedChecks"/> to pojemność godziny
/// (3600 s / interval) — różnica między nią a up + down to czas bez danych
/// (bieżąca godzina w toku, monitor utworzony w trakcie godziny, pauza).
/// </summary>
public sealed record HourlyCheckSummary(
    DateTimeOffset Hour,
    int UpChecks,
    int DownChecks,
    int ExpectedChecks);

public static class HourlyHistory
{
    public const int DefaultHours = 24;

    /// <summary>Początek okna <paramref name="hours"/> pełnych godzin UTC kończącego się bieżącą (w toku).</summary>
    public static DateTimeOffset WindowStart(DateTimeOffset now, int hours = DefaultHours) =>
        TimeWindows.FloorToHour(now).AddHours(-(hours - 1));

    /// <summary>Składa ciągłą historię (godziny bez danych wypełnione zerami) z wierszy agregatu.</summary>
    public static IReadOnlyList<HourlyCheckSummary> Build(
        Monitor monitor,
        DateTimeOffset windowStart,
        IReadOnlyDictionary<(Guid MonitorId, DateTimeOffset Bucket), MonitorCheckHourly> rollups,
        int hours = DefaultHours)
    {
        var capacity = Math.Max(1, 3600 / monitor.IntervalSeconds);

        return Enumerable.Range(0, hours)
            .Select(offset =>
            {
                var hour = windowStart.AddHours(offset);
                if (!rollups.TryGetValue((monitor.Id, hour), out var bucket))
                    return new HourlyCheckSummary(hour, 0, 0, capacity);

                var up = (int)bucket.UpChecks;
                var down = (int)(bucket.TotalChecks - bucket.UpChecks);

                // przy zmianie intervalu albo jitterze schedulera checków może być więcej niż pojemność
                return new HourlyCheckSummary(hour, up, down, Math.Max(capacity, up + down));
            })
            .ToList();
    }

    public static IReadOnlyDictionary<(Guid MonitorId, DateTimeOffset Bucket), MonitorCheckHourly> Index(
        IEnumerable<MonitorCheckHourly> rollups) =>
        rollups.ToDictionary(r => (r.MonitorId, r.Bucket));
}
