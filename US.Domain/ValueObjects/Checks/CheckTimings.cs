namespace US.Domain.ValueObjects.Checks;

public sealed record CheckTimings(
    int? DnsMs = null,
    int? ConnectMs = null,
    int? TlsMs = null,
    int? TtfbMs = null,
    int? TransferMs = null);
