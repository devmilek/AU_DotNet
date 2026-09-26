using Microsoft.AspNetCore.Mvc;
using AU.Application.Common;
using AU.Application.Monitors.Queries.GetAllMonitors;

namespace AU.Api.Controllers.Monitors.Requests;

public sealed class GetMonitorsRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
    [FromQuery(Name = "sortBy")]
    public MonitorSortBy SortBy { get; init; } = MonitorSortBy.CreatedAt;
    [FromQuery(Name = "sortOrder")]
    public SortOrder SortOrder { get; init; } = SortOrder.Desc;

    /// <summary>Lista typów oddzielona przecinkami, np. <c>http,tcp,ping</c>.</summary>
    [FromQuery(Name = "type")]
    public string? Type { get; init; }

    /// <summary>Szuka po nazwie i targecie monitora.</summary>
    [FromQuery(Name = "search")]
    public string? Search { get; init; }
}
