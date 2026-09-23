using System.Text.Json.Serialization;

namespace US.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChannelType
{
    Email,
    Discord,
    Slack,
    Webhook
}