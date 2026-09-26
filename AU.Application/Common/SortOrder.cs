using System.Text.Json.Serialization;

namespace AU.Application.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortOrder
{
    Asc,
    Desc
}
