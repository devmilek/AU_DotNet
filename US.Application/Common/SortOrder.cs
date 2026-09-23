using System.Text.Json.Serialization;

namespace US.Application.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortOrder
{
    Asc,
    Desc
}
