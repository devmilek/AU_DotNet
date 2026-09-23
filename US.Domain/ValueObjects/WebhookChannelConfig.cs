namespace US.Domain.ValueObjects;

public sealed class WebhookChannelConfig : ChannelConfig
{
    public required string Url { get; init; }
    public string? Secret { get; init; }
}