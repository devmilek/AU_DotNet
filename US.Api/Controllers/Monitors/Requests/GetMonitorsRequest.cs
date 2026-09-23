using US.Application.Common;
using US.Application.Monitors.Queries.GetAllMonitors;

namespace US.Api.Controllers.Monitors.Requests;

public sealed class GetMonitorsRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public MonitorSortBy SortBy { get; init; } = MonitorSortBy.CreatedAt;
    public SortOrder SortOrder { get; init; } = SortOrder.Desc;

    /// <summary>Lista typów oddzielona przecinkami, np. <c>http,tcp,ping</c>.</summary>
    public string? Type { get; init; }

    /// <summary>Szuka po nazwie i targecie monitora.</summary>
    public string? Search { get; init; }
}
