using System.Text.Json.Serialization;

namespace US.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MonitorType
{
    Http,
    Ping,
    Ssl,
    Tcp
}