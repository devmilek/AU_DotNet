namespace US.Domain.ValueObjects;

public sealed class WebhookChannelConfig : ChannelConfig
{
    public required string Url { get; init; }
    /// <summary>Sekret do podpisywania payloadu — zaszyfrowany, odszyfrowywany dopiero przy wysyłce.</summary>
    public ProtectedSecret? Secret { get; init; }
}