using System.Net;

namespace AU.Infrastructure.Checkers;

internal static class HostResolver
{
    public static async Task<IPAddress[]> ResolveAsync(string host, CancellationToken ct)
    {
        return IPAddress.TryParse(host, out var address)
            ? [address]
            : await Dns.GetHostAddressesAsync(host, ct);
    }
}
