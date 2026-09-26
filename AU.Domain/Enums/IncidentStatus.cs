using System.Text.Json.Serialization;

namespace AU.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IncidentStatus
{
    Ongoing,
    Resolved,
    Acknowledged,
}