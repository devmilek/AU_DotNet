using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using US.Api.Controllers.Monitors.Responses;
using US.Application.Monitors.Queries.GetMonitorResponseTimes;
using US.Application.Monitors.Queries.GetMonitorStatus;
using US.Application.Monitors.Queries.GetMonitorUptime;
using Wolverine;

namespace US.Api.Controllers;

/// <summary>
/// Odczyty stanu i statystyk pojedynczego monitora (dashboard szczegółów).
/// Osobne zasoby zamiast jednego "overview", żeby front mógł je cache'ować i odświeżać niezależnie
/// (status często, uptime rzadziej, wykres tylko przy zmianie zakresu).
/// </summary>
[ApiController]
[Tags("Monitors")]
[Produces("application/json")]
[Route("api/{orgId:guid}/monitors/{monitorId:guid}")]
[Authorize(Policy = OrgPolicies.Member)]
public class MonitorStatisticsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("status", Name = "GetMonitorStatus")]
    [ProducesResponseType<MonitorStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(Guid orgId, Guid monitorId, CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<MonitorStatusResult>(
            new GetMonitorStatusQuery(orgId, monitorId), cancellationToken);
        return Ok(MonitorStatusResponse.FromResult(result));
    }

    [HttpGet("uptime", Name = "GetMonitorUptime")]
    [ProducesResponseType<MonitorUptimeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUptime(Guid orgId, Guid monitorId, CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<MonitorUptimeResult>(
            new GetMonitorUptimeQuery(orgId, monitorId), cancellationToken);
        return Ok(MonitorUptimeResponse.FromResult(result));
    }

    [HttpGet("response-times", Name = "GetMonitorResponseTimes")]
    [ProducesResponseType<MonitorResponseTimesResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResponseTimes(
        Guid orgId,
        Guid monitorId,
        [FromQuery(Name = "range")] ResponseTimeRange range = ResponseTimeRange.Last24Hours,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(range))
        {
            ModelState.AddModelError("range", "Nieznany zakres.");
            return ValidationProblem(ModelState);
        }

        var result = await bus.InvokeAsync<MonitorResponseTimesResult>(
            new GetMonitorResponseTimesQuery(orgId, monitorId, range), cancellationToken);
        return Ok(MonitorResponseTimesResponse.FromResult(result));
    }
}
