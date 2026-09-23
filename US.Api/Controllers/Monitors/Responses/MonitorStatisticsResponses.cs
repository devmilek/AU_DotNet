using US.Application.Checks.Statistics;
using US.Application.Monitors.Queries.GetMonitorResponseTimes;
using US.Application.Monitors.Queries.GetMonitorStatus;
using US.Application.Monitors.Queries.GetMonitorUptime;
using US.Domain.Enums;

namespace US.Api.Controllers.Monitors.Responses;

/// <param name="Since">Od kiedy monitor jest w obecnym stanie; null przy pauzie.</param>
public sealed record MonitorStatusResponse(
    MonitorStatus Status,
    DateTimeOffset? Since,
    LastCheckResponse? LastCheck)
{
    public static MonitorStatusResponse FromResult(MonitorStatusResult result) => new(
        result.Status,
        result.Since,
        result.LastCheck is { } check
            ? new LastCheckResponse(check.CheckedAt, check.Status == CheckStatus.UP, check.ResponseTimeMs, check.StatusCode, check.ErrorMessage)
            : null);
}

public sealed record LastCheckResponse(
    DateTimeOffset CheckedAt,
    bool IsUp,
    int? ResponseTimeMs,
    int? StatusCode,
    string? ErrorMessage);

/// <param name="UptimeRatio">0–1; null, gdy w okresie nie było checków.</param>
public sealed record UptimePeriodResponse(
    UptimePeriod Period,
    DateTimeOffset From,
    long TotalChecks,
    long UpChecks,
    double? UptimeRatio);

public sealed record MonitorUptimeResponse(
    IReadOnlyList<UptimePeriodResponse> Periods,
    IReadOnlyList<HourlyCheckSummaryResponse> Last24Hours)
{
    public static MonitorUptimeResponse FromResult(MonitorUptimeResult result) => new(
        result.Periods
            .Select(p => new UptimePeriodResponse(p.Period, p.From, p.TotalChecks, p.UpChecks, p.UptimeRatio))
            .ToList(),
        result.Last24Hours.Select(HourlyCheckSummaryResponse.From).ToList());
}

/// <summary>Punkt wykresu czasu odpowiedzi (tylko udane checki); null = brak danych w kubełku.</summary>
public sealed record ResponseTimePointResponse(
    DateTimeOffset Timestamp,
    double? AverageMs,
    int? MinimumMs,
    int? MaximumMs);

public sealed record ResponseTimeSummaryResponse(
    double? AverageMs,
    int? MinimumMs,
    int? MaximumMs);

public sealed record MonitorResponseTimesResponse(
    ResponseTimeRange Range,
    DateTimeOffset From,
    int BucketSeconds,
    IReadOnlyList<ResponseTimePointResponse> Points,
    ResponseTimeSummaryResponse Summary)
{
    public static MonitorResponseTimesResponse FromResult(MonitorResponseTimesResult result) => new(
        result.Range,
        result.From,
        result.BucketSeconds,
        result.Points
            .Select(p => new ResponseTimePointResponse(p.Timestamp, p.AverageMs, p.MinimumMs, p.MaximumMs))
            .ToList(),
        new ResponseTimeSummaryResponse(result.Summary.AverageMs, result.Summary.MinimumMs, result.Summary.MaximumMs));
}
