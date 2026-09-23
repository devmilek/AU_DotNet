using System.Text.Json.Serialization;

namespace US.Application.Monitors.Queries.GetAllMonitors;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MonitorSortBy
{
    CreatedAt,
    Status
}
