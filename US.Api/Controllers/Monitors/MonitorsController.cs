using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using US.Api.Controllers.Monitors.Requests;
using US.Application.Channels.Commands.AssignChannelToMonitor;
using US.Application.Channels.Commands.SendTestNotification;
using US.Application.Channels.Commands.UnassignChannelFromMonitor;
using US.Application.Monitors.Commands.CreateMonitor;
using US.Application.Monitors.Queries.GetAllMonitors;
using US.Application.Monitors.Queries.GetMonitor;
using Wolverine;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Api.Controllers;

[ApiController]
[Tags("Monitors")]
[Produces("application/json")]
[Route("api/{orgId:guid}/monitors")]
[Authorize(Policy = OrgPolicies.Member)]
public class MonitorsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetMonitor")]
    [ProducesResponseType<Monitor>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid orgId, Guid id)
    {
        var monitor = await bus.InvokeAsync<Monitor?>(new GetMonitorQuery(orgId, id));
        if (monitor == null)
        {
            return NotFound();
        }
        return Ok(monitor);
    }
    
    [HttpGet(Name = "GetAllMonitors")]
    [ProducesResponseType<IReadOnlyList<Monitor>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid orgId)
    {
        var monitors = await bus.InvokeAsync<IReadOnlyList<Monitor>>(new GetAllMonitorsQuery(orgId));
        return Ok(monitors);
    }

    [HttpPost(Name = "CreateMonitor")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> CreateMonitor(Guid orgId, CreateMonitorRequest request)
    {
        var command = new CreateMonitorCommand(orgId, request.Name, request.Type, request.Target, request.IntervalSeconds, request.TimeoutMs, request.AlertThreshold, request.RecoveryThreshold);
        var result = await bus.InvokeAsync<Guid>(command);
        return CreatedAtAction(nameof(Get), new { orgId, id = result }, result);
    }

    [HttpPost("{monitorId:guid}/channels/{channelId:guid}", Name = "AssignChannelToMonitor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignChannelToMonitor(Guid orgId, Guid monitorId, Guid channelId)
    {
        var command = new AssignChannelToMonitorCommand(orgId, monitorId, channelId);
        await bus.InvokeAsync(command);
        return NoContent();
    }
    
    [HttpDelete("{monitorId:guid}/channels/{channelId:guid}", Name = "UnassignChannelFromMonitor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnassignChannelFromMonitor(Guid orgId, Guid monitorId, Guid channelId)
    {
        var command = new UnassingChannelFromMonitorCommand(orgId, monitorId, channelId);
        await bus.InvokeAsync(command);
        return NoContent();
    }

    [HttpPost("{monitorId:guid}/test-notification", Name = "SendTestNotification")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> SendTestNotification(Guid orgId, Guid monitorId)
    {
        var command = new SendTestNotificationCommand(orgId, monitorId);
        await bus.InvokeAsync(command);
        return Accepted();
    }
}
