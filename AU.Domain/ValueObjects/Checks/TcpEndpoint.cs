using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AU.Domain.ValueObjects.Checks;

public sealed record TcpEndpoint(string Host, int Port)
{
    public static bool TryParse(string? value, [NotNullWhen(true)] out TcpEndpoint? endpoint)
    {
        endpoint = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value.Trim();
        var separator = trimmed.LastIndexOf(':');
        if (separator <= 0 || separator == trimmed.Length - 1) return false;

        var host = trimmed[..separator];
        var portText = trimmed[(separator + 1)..];

        if (host.StartsWith('[') && host.EndsWith(']'))
        {
            host = host[1..^1];
            if (!IPAddress.TryParse(host, out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                return false;
        }
        else if (host.Contains(':'))
        {
            return false;
        }

        if (!int.TryParse(portText, out var port) || port is < 1 or > 65535) return false;
        if (Uri.CheckHostName(host) == UriHostNameType.Unknown) return false;

        endpoint = new TcpEndpoint(host, port);
        return true;
    }

    public override string ToString() =>
        Host.Contains(':') ? $"[{Host}]:{Port}" : $"{Host}:{Port}";
}
