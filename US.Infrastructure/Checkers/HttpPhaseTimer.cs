using System.Diagnostics;
using System.Net.Sockets;
using US.Domain.ValueObjects.Checks;

namespace US.Infrastructure.Checkers;

internal sealed class HttpPhaseTimer
{
    private TimeSpan _dns;
    private TimeSpan _connect;
    private TimeSpan? _tls;
    private long? _connectedAt;
    private long? _requestSentAt;
    private long? _firstByteAt;
    private long? _completedAt;

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken ct)
    {
        var endpoint = context.DnsEndPoint;

        var dnsStart = Stopwatch.GetTimestamp();
        var addresses = await HostResolver.ResolveAsync(endpoint.Host, ct);
        _dns += Stopwatch.GetElapsedTime(dnsStart);

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            var connectStart = Stopwatch.GetTimestamp();
            await socket.ConnectAsync(addresses, endpoint.Port, ct);
            _connect += Stopwatch.GetElapsedTime(connectStart);
            _connectedAt = Stopwatch.GetTimestamp();

            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    public ValueTask<Stream> FilterPlaintextStream(SocketsHttpPlaintextStreamFilterContext context, CancellationToken ct)
    {
        if (_connectedAt is { } connectedAt
            && context.InitialRequestMessage.RequestUri?.Scheme == Uri.UriSchemeHttps)
        {
            _tls = (_tls ?? TimeSpan.Zero) + Stopwatch.GetElapsedTime(connectedAt);
        }

        _connectedAt = null;
        return ValueTask.FromResult<Stream>(new PhaseTimingStream(context.PlaintextStream, this));
    }

    public void RequestWritten()
    {
        _requestSentAt = Stopwatch.GetTimestamp();
        _firstByteAt = null;
    }

    public void ResponseByteReceived()
    {
        _firstByteAt ??= Stopwatch.GetTimestamp();
    }

    public void Completed()
    {
        _completedAt = Stopwatch.GetTimestamp();
    }

    public CheckTimings ToCheckTimings() => new(
        DnsMs: ToMilliseconds(_dns),
        ConnectMs: ToMilliseconds(_connect),
        TlsMs: _tls is { } tls ? ToMilliseconds(tls) : null,
        TtfbMs: Between(_requestSentAt, _firstByteAt),
        TransferMs: Between(_firstByteAt, _completedAt));

    private static int? Between(long? start, long? end) =>
        start is { } from && end is { } to && to >= from
            ? ToMilliseconds(Stopwatch.GetElapsedTime(from, to))
            : null;

    private static int ToMilliseconds(TimeSpan value) =>
        (int)Math.Round(value.TotalMilliseconds, MidpointRounding.AwayFromZero);
}
