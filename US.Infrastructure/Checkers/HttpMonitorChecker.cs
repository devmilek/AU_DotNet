using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using US.Application.Abstractions;
using US.Application.Checks;
using US.Domain.Entities;
using US.Domain.Enums;
using US.Domain.ValueObjects.Checks;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Checkers;
 
public class HttpMonitorChecker(IHttpClientFactory httpClientFactory, ISecretProtector secretProtector) : IMonitorChecker
{
    // AllowAutoRedirect to ustawienie handlera, nie requestu — stąd dwa osobne klienty
    public const string FollowRedirectsClient = "http-check-follow";
    public const string NoRedirectsClient = "http-check-no-follow";

    public MonitorType Type => MonitorType.Http;
    
    public async Task<Check> CheckAsync(Monitor monitor)
    {
        var config = monitor.Config as HttpCheckConfig ?? HttpCheckConfig.Default;
        var httpClient = httpClientFactory.CreateClient(
            config.FollowRedirects ? FollowRedirectsClient : NoRedirectsClient);

        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(monitor.TimeoutMs));
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = CreateRequest(monitor.Target, config);
            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);

            stopwatch.Stop();

            var statusCode = (int)response.StatusCode;
            var isUp = config.IsAccepted(statusCode);

            return Check.Create(
                monitor.Id,
                DateTimeOffset.UtcNow,
                isUp ? CheckStatus.UP : CheckStatus.DOWN,
                (int)stopwatch.ElapsedMilliseconds,
                statusCode,
                isUp ? null : $"Unexpected status code {statusCode} {response.ReasonPhrase}".TrimEnd());
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            stopwatch.Stop();

            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, (int)stopwatch.ElapsedMilliseconds, null,
                $"Timeout after {monitor.TimeoutMs} ms");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, (int)stopwatch.ElapsedMilliseconds, null, ex.Message);
        }
    }

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
