using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using US.Application.Abstractions;
using US.Application.Checks;
using US.Domain.Entities;
using US.Domain.Enums;
using US.Domain.ValueObjects.Checks;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Checkers;

public class HttpMonitorChecker(ISecretProtector secretProtector) : IMonitorChecker
{
    private const int MaxBodyBytes = 5 * 1024 * 1024;
    private const int ReadBufferBytes = 16 * 1024;

    public MonitorType Type => MonitorType.Http;

    public async Task<Check> CheckAsync(Monitor monitor)
    {
        var config = monitor.Config as HttpCheckConfig ?? HttpCheckConfig.Default;
        var timer = new HttpPhaseTimer();

        using var httpClient = CreateClient(config, timer);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(monitor.TimeoutMs));
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            using var request = CreateRequest(monitor.Target, config);
            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);

            await DrainBodyAsync(response, timeout.Token);
            timer.Completed();

            var statusCode = (int)response.StatusCode;
            var isUp = config.IsAccepted(statusCode);

            return Check.Create(
                monitor.Id,
                DateTimeOffset.UtcNow,
                isUp ? CheckStatus.UP : CheckStatus.DOWN,
                ElapsedMilliseconds(startedAt),
                statusCode,
                isUp ? null : $"Unexpected status code {statusCode} {response.ReasonPhrase}".TrimEnd(),
                timer.ToCheckTimings());
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, ElapsedMilliseconds(startedAt), null,
                $"Timeout after {monitor.TimeoutMs} ms", timer.ToCheckTimings());
        }
        catch (Exception ex)
        {
            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, ElapsedMilliseconds(startedAt), null,
                ex.Message, timer.ToCheckTimings());
        }
    }

    private static HttpClient CreateClient(HttpCheckConfig config, HttpPhaseTimer timer)
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = config.FollowRedirects,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.Zero,
            ConnectCallback = timer.ConnectAsync,
            PlaintextStreamFilter = timer.FilterPlaintextStream
        };

        return new HttpClient(handler, disposeHandler: true) { Timeout = Timeout.InfiniteTimeSpan };
    }

    private static async Task DrainBodyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        await using var body = await response.Content.ReadAsStreamAsync(ct);
        var buffer = ArrayPool<byte>.Shared.Rent(ReadBufferBytes);

        try
        {
            var total = 0;
            int read;
            while (total < MaxBodyBytes && (read = await body.ReadAsync(buffer.AsMemory(0, ReadBufferBytes), ct)) > 0)
                total += read;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static int ElapsedMilliseconds(long startedAt) =>
        (int)Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

    private HttpRequestMessage CreateRequest(string target, HttpCheckConfig config)
    {
        var request = new HttpRequestMessage(ToHttpMethod(config.Method), target);

        // sekret odszyfrowany tylko na czas budowania requestu, nigdzie nie zapisywany
        request.Headers.Authorization = config.Auth switch
        {
            BasicHttpAuth basic => new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basic.Username}:{secretProtector.Unprotect(basic.Password)}"))),
            BearerHttpAuth bearer => new AuthenticationHeaderValue("Bearer", secretProtector.Unprotect(bearer.Token)),
            _ => null
        };

        return request;
    }

    private static HttpMethod ToHttpMethod(HttpCheckMethod method) => method switch
    {
        HttpCheckMethod.Get => HttpMethod.Get,
        HttpCheckMethod.Head => HttpMethod.Head,
        HttpCheckMethod.Post => HttpMethod.Post,
        HttpCheckMethod.Put => HttpMethod.Put,
        HttpCheckMethod.Patch => HttpMethod.Patch,
        HttpCheckMethod.Delete => HttpMethod.Delete,
        HttpCheckMethod.Options => HttpMethod.Options,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
    };
}
