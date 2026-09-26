using AU.Domain.Enums;
using AU.Application.Common;

namespace AU.Application.Monitors.Queries.GetAllMonitors;

public record GetAllMonitorsQuery(
    Guid OrganizationId,
    int Page = 1,
    int PageSize = 20,
    MonitorSortBy SortBy = MonitorSortBy.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc,
    IReadOnlyList<MonitorType>? Types = null,
    string? Search = null);
