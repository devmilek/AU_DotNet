using System.Text.Json.Serialization;

namespace AU.Domain.ValueObjects;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EmailChannelConfig), "email")]
[JsonDerivedType(typeof(DiscordChannelConfig), "discord")]
[JsonDerivedType(typeof(WebhookChannelConfig), "webhook")]
public abstract class ChannelConfig
{
}