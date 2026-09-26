using System.Text.Json.Serialization;

namespace AU.Application.Monitors.Queries.GetAllMonitors;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MonitorSortBy
{
    CreatedAt,
    Status
}
