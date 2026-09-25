using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using US.Api.Controllers.Monitors.Requests;
using US.Api.Controllers.Monitors.Responses;
using US.Application.Channels.Commands.AssignChannelToMonitor;
using US.Application.Channels.Commands.SendTestNotification;
using US.Application.Channels.Commands.UnassignChannelFromMonitor;
using US.Application.Common;
using US.Application.MaintenanceWindows.Commands.AssignMaintenanceWindowToMonitor;
using US.Application.MaintenanceWindows.Commands.UnassignMaintenanceWindowFromMonitor;
using US.Application.Monitors.Commands.CreateMonitor;
using US.Application.Monitors.Commands.PauseMonitor;
using US.Application.Monitors.Commands.ResumeMonitor;
using US.Application.Monitors.Queries.GetAllMonitors;
using US.Application.Monitors.Queries.GetMonitor;
using US.Application.Monitors.Queries.GetMonitorStatuses;
using US.Domain.Enums;
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
    [ProducesResponseType<MonitorResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid orgId, Guid id)
    {
        var monitor = await bus.InvokeAsync<Monitor?>(new GetMonitorQuery(orgId, id));
        if (monitor == null)
        {
            return NotFound();
        }
        return Ok(MonitorResponse.From(monitor));
    }
    
    [HttpGet("statuses", Name = "GetMonitorStatuses")]
    [ProducesResponseType<IReadOnlyList<MonitorStatusSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatuses(Guid orgId, CancellationToken cancellationToken)
    {
        var summaries = await bus.InvokeAsync<IReadOnlyList<MonitorStatusSummary>>(
            new GetMonitorStatusesQuery(orgId), cancellationToken);
        return Ok(summaries.Select(MonitorStatusSummaryResponse.From).ToList());
    }

    [HttpGet(Name = "GetAllMonitors")]
    [ProducesResponseType<PagedResult<MonitorListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid orgId, [FromQuery] GetMonitorsRequest request)
    {
        var types = new List<MonitorType>();
        foreach (var value in (request.Type ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<MonitorType>(value, ignoreCase: true, out var type) || !Enum.IsDefined(type))
            {
                ModelState.AddModelError(nameof(request.Type), $"Nieznany typ monitora: '{value}'.");
                continue;
            }
            types.Add(type);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var query = new GetAllMonitorsQuery(
            orgId,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortOrder,
            types,
            request.Search);

        var monitors = await bus.InvokeAsync<PagedResult<MonitorListItem>>(query);
        return Ok(new PagedResult<MonitorListItemResponse>(
            monitors.Items.Select(MonitorListItemResponse.From).ToList(),
            monitors.Page,
            monitors.PageSize,
            monitors.TotalCount));
    }

    [HttpPost(Name = "CreateMonitor")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> CreateMonitor(Guid orgId, CreateMonitorRequest request)
    {
        var command = new CreateMonitorCommand(orgId, request.Name, request.Type, request.Target, request.IntervalSeconds, request.TimeoutMs, request.AlertThreshold, request.RecoveryThreshold, request.Http);
        var result = await bus.InvokeAsync<Guid>(command);
        return CreatedAtAction(nameof(Get), new { orgId, id = result }, result);
    }

    [HttpPost("{monitorId:guid}/pause", Name = "PauseMonitor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> Pause(Guid orgId, Guid monitorId, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new PauseMonitorCommand(orgId, monitorId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{monitorId:guid}/resume", Name = "ResumeMonitor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> Resume(Guid orgId, Guid monitorId, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new ResumeMonitorCommand(orgId, monitorId), cancellationToken);
        return NoContent();
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

    [HttpPost("{monitorId:guid}/maintenance-windows/{windowId:guid}", Name = "AssignMaintenanceWindowToMonitor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> AssignMaintenanceWindowToMonitor(
        Guid orgId, Guid monitorId, Guid windowId, CancellationToken cancellationToken)
    {
        var command = new AssignMaintenanceWindowToMonitorCommand(orgId, monitorId, windowId);
        await bus.InvokeAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{monitorId:guid}/maintenance-windows/{windowId:guid}", Name = "UnassignMaintenanceWindowFromMonitor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> UnassignMaintenanceWindowFromMonitor(
        Guid orgId, Guid monitorId, Guid windowId, CancellationToken cancellationToken)
    {
        var command = new UnassignMaintenanceWindowFromMonitorCommand(orgId, monitorId, windowId);
        await bus.InvokeAsync(command, cancellationToken);
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
