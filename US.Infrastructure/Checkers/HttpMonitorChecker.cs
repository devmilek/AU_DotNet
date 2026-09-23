using System.Diagnostics;
using US.Application.Checks;
using US.Domain.Entities;
using US.Domain.Enums;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Checkers;
 
public class HttpMonitorChecker(HttpClient httpClient) : IMonitorChecker
{
    public MonitorType Type => MonitorType.Http;
    
    public async Task<Check> CheckAsync(Monitor monitor)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response =
                await httpClient.GetAsync(monitor.Target);

            stopwatch.Stop();

            var now = DateTimeOffset.UtcNow;
            var status = response.IsSuccessStatusCode ? CheckStatus.UP : CheckStatus.DOWN;

            return Check.Create(monitor.Id, now, status, (int)stopwatch.ElapsedMilliseconds, (int)response.StatusCode, response.IsSuccessStatusCode ? null : response.ReasonPhrase);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return Check.Create(monitor.Id, DateTimeOffset.UtcNow, CheckStatus.DOWN, (int)stopwatch.ElapsedMilliseconds, null, ex.Message);
        }
    }
}