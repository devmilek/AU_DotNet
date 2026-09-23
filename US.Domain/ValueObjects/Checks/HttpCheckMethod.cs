using System.Text.Json.Serialization;

namespace US.Domain.ValueObjects.Checks;

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
