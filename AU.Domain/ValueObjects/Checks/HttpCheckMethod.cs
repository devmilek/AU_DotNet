using System.Text.Json.Serialization;

namespace AU.Domain.ValueObjects.Checks;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HttpCheckMethod
{
    Get,
    Head,
    Post,
    Put,
    Patch,
    Delete,
    Options
}
