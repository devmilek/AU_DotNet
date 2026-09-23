using System.Text.Json.Serialization;
using US.Domain.Enums;

namespace US.Application.Monitors.Queries.GetMonitorStatus;

public sealed record GetMonitorStatusQuery(Guid OrganizationId, Guid MonitorId);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MonitorStatus
{
    /// <summary>Monitor aktywny, ale nie ma jeszcze żadnego checka.</summary>
    Pending,
    Up,
    /// <summary>Trwa incydent (przekroczony próg alertu) — nie sam pojedynczy nieudany check.</summary>
    Down,
    Paused
}

/// <param name="Since">Od kiedy monitor jest w obecnym stanie; null, gdy nie da się tego ustalić (np. pauza).</param>
public sealed record MonitorStatusResult(
    MonitorStatus Status,
    DateTimeOffset? Since,
    LastCheckResult? LastCheck);

public sealed record LastCheckResult(
    DateTimeOffset CheckedAt,
    CheckStatus Status,
    int? ResponseTimeMs,
    int? StatusCode,
    string? ErrorMessage);
