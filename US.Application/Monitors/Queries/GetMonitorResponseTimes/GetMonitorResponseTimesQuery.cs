using System.Text.Json.Serialization;
using US.Application.Checks.Statistics;

namespace US.Application.Monitors.Queries.GetMonitorResponseTimes;

public sealed record GetMonitorResponseTimesQuery(Guid OrganizationId, Guid MonitorId, ResponseTimeRange Range);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResponseTimeRange
{
    Last24Hours,
    Last7Days,
    Last30Days
}

/// <summary>Punkt wykresu; wartości null = brak udanych checków w kubełku (przerwa na wykresie).</summary>
public sealed record ResponseTimePoint(
    DateTimeOffset Timestamp,
    double? AverageMs,
    int? MinimumMs,
    int? MaximumMs,
    PhaseTimings? Phases);

public sealed record ResponseTimeSummary(
    double? AverageMs,
    int? MinimumMs,
    int? MaximumMs,
    PhaseTimings? Phases);

public sealed record MonitorResponseTimesResult(
    ResponseTimeRange Range,
    DateTimeOffset From,
    int BucketSeconds,
    IReadOnlyList<ResponseTimePoint> Points,
    ResponseTimeSummary Summary);
