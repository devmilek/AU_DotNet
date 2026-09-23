namespace US.Domain.ValueObjects;

public sealed class DiscordChannelConfig : ChannelConfig
{
    /// <summary>URL webhooka Discorda zawiera token (kto go zna, może pisać na kanał), więc też jest szyfrowany.</summary>
    public required ProtectedSecret WebhookUrl { get; init; }
}