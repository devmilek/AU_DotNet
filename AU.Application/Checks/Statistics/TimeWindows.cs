namespace AU.Application.Checks.Statistics;

/// <summary>
/// Wyrównywanie czasu do kubełków. Musi dawać te same granice co <c>time_bucket</c> w TimescaleDB
/// (dla interwałów będących dzielnikiem doby — liczone od północy UTC).
/// </summary>
public static class TimeWindows
{
    public static DateTimeOffset FloorToHour(DateTimeOffset value) => Floor(value, TimeSpan.FromHours(1));

    public static DateTimeOffset FloorToDay(DateTimeOffset value) => Floor(value, TimeSpan.FromDays(1));

    public static DateTimeOffset Floor(DateTimeOffset value, TimeSpan bucket)
    {
        var utc = value.ToUniversalTime();
        return new DateTimeOffset(utc.UtcTicks - utc.UtcTicks % bucket.Ticks, TimeSpan.Zero);
    }
}
