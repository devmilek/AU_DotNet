namespace US.Domain.ValueObjects;

public sealed class DiscordChannelConfig : ChannelConfig
{
    public required string WebhookUrl { get; init; }
}