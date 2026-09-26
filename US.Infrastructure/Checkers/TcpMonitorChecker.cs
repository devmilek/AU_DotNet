using System.Diagnostics;
using System.Net.Sockets;
using US.Application.Checks;
using US.Domain.Entities;
using US.Domain.Enums;
using US.Domain.ValueObjects.Checks;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Checkers;

public class TcpMonitorChecker : IMonitorChecker
{
    public MonitorType Type => MonitorType.Tcp;

    public async Task<Check> CheckAsync(Monitor monitor)
    {
        if (!TcpEndpoint.TryParse(monitor.Target, out var endpoint))
        {
            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, null, null,
                $"Invalid TCP target \"{monitor.Target}\". Expected host:port.");
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(monitor.TimeoutMs));
        var startedAt = Stopwatch.GetTimestamp();
        int? dnsMs = null;
        int? connectMs = null;

        try
        {
            var dnsStart = Stopwatch.GetTimestamp();
            var addresses = await HostResolver.ResolveAsync(endpoint.Host, timeout.Token);
            dnsMs = ElapsedMilliseconds(dnsStart);

            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            var connectStart = Stopwatch.GetTimestamp();
            await socket.ConnectAsync(addresses, endpoint.Port, timeout.Token);
            connectMs = ElapsedMilliseconds(connectStart);

            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.UP, ElapsedMilliseconds(startedAt), null, null,
                new CheckTimings(DnsMs: dnsMs, ConnectMs: connectMs));
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, ElapsedMilliseconds(startedAt), null,
                $"Timeout after {monitor.TimeoutMs} ms", new CheckTimings(DnsMs: dnsMs, ConnectMs: connectMs));
        }
        catch (SocketException ex)
        {
            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, ElapsedMilliseconds(startedAt), null,
                $"{ex.SocketErrorCode}: {ex.Message}", new CheckTimings(DnsMs: dnsMs, ConnectMs: connectMs));
        }
    }

    private static int ElapsedMilliseconds(long startedAt) =>
        (int)Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, MidpointRounding.AwayFromZero);
}
