using System.Text.Json.Serialization;
using US.Application.Checks.Statistics;

namespace US.Application.Monitors.Queries.GetMonitorUptime;

public sealed record GetMonitorUptimeQuery(Guid OrganizationId, Guid MonitorId);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UptimePeriod
{
    Last24Hours,
    Last7Days,
    Last30Days,
    Last365Days
}

public sealed record UptimePeriodResult(
    UptimePeriod Period,
    DateTimeOffset From,
    long TotalChecks,
    long UpChecks,
    double? UptimeRatio);

public sealed record MonitorUptimeResult(
    IReadOnlyList<UptimePeriodResult> Periods,
    IReadOnlyList<HourlyCheckSummary> Last24Hours);
