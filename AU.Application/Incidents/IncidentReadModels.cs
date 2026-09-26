using System.Text.Json.Serialization;
using AU.Application.Abstractions;
using AU.Domain.Enums;

namespace AU.Application.Incidents;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IncidentStatusFilter
{
    All,
    Ongoing,
    Resolved
}

public sealed record IncidentRow(
    Guid Id,
    string? Name,
    string? Cause,
    Guid MonitorId,
    string MonitorName,
    string MonitorTarget,
    IncidentStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedByUserId,
    DateTimeOffset? ResolvedAt,
    int FailedChecksCount,
    bool StartedInMaintenance);

public sealed record IncidentMonitor(Guid Id, string Name, string Target);

public sealed record IncidentAcknowledgement(DateTimeOffset At, Guid? UserId, string? UserName);

public sealed record IncidentResponse(
    Guid Id,
    string Name,
    bool HasCustomName,
    string? Cause,
    IncidentMonitor Monitor,
    IncidentStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? ResolvedAt,
    int FailedChecksCount,
    bool StartedInMaintenance,
    IncidentAcknowledgement? Acknowledgement)
{
    public static string DefaultName(string monitorName) => $"{monitorName} is down";

    public static async Task<IReadOnlyList<IncidentResponse>> FromRowsAsync(
        IReadOnlyList<IncidentRow> rows,
        IUserLookup userLookup,
        CancellationToken ct)
    {
        var userIds = rows
            .Where(r => r.AcknowledgedByUserId is not null)
            .Select(r => r.AcknowledgedByUserId!.Value)
            .Distinct()
            .ToList();

        var users = userIds.Count == 0
            ? new Dictionary<Guid, UserSummary>()
            : await userLookup.GetUsersAsync(userIds, ct);

        return rows.Select(row => From(row, users)).ToList();
    }

    private static IncidentResponse From(IncidentRow row, IReadOnlyDictionary<Guid, UserSummary> users)
    {
        IncidentAcknowledgement? acknowledgement = null;
        if (row.AcknowledgedAt is { } acknowledgedAt)
        {
            UserSummary? user = null;
            if (row.AcknowledgedByUserId is { } userId)
                users.TryGetValue(userId, out user);

            acknowledgement = new IncidentAcknowledgement(
                acknowledgedAt, row.AcknowledgedByUserId, user?.DisplayName ?? user?.Email);
        }

        return new IncidentResponse(
            row.Id,
            row.Name ?? DefaultName(row.MonitorName),
            row.Name is not null,
            row.Cause,
            new IncidentMonitor(row.MonitorId, row.MonitorName, row.MonitorTarget),
            row.Status,
            row.StartedAt,
            row.ResolvedAt,
            row.FailedChecksCount,
            row.StartedInMaintenance,
            acknowledgement);
    }
}
