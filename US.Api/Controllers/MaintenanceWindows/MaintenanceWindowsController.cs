using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using US.Api.Controllers.MaintenanceWindows.Requests;
using US.Api.Controllers.MaintenanceWindows.Responses;
using US.Application.MaintenanceWindows;
using US.Application.MaintenanceWindows.Commands.CreateMaintenanceWindow;
using US.Application.MaintenanceWindows.Commands.DeleteMaintenanceWindow;
using US.Application.MaintenanceWindows.Commands.RenameMaintenanceWindow;
using US.Application.MaintenanceWindows.Commands.SetMaintenanceWindowMonitors;
using US.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowPolicy;
using US.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowSchedule;
using US.Application.MaintenanceWindows.Queries.GetMaintenanceOccurrences;
using US.Application.MaintenanceWindows.Queries.GetMaintenanceWindows;
using Wolverine;

namespace US.Api.Controllers;

[ApiController]
[Tags("MaintenanceWindows")]
[Produces("application/json")]
[Route("api/{orgId:guid}/maintenance-windows")]
[Authorize(Policy = OrgPolicies.Member)]
public class MaintenanceWindowsController(IMessageBus bus) : ControllerBase
{
    [HttpGet(Name = "GetMaintenanceWindows")]
    [ProducesResponseType<IReadOnlyList<MaintenanceWindowListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid orgId, CancellationToken cancellationToken)
    {
        var rows = await bus.InvokeAsync<IReadOnlyList<MaintenanceWindowListRow>>(
            new GetMaintenanceWindowsQuery(orgId), cancellationToken);
        return Ok(rows.Select(MaintenanceWindowListItemResponse.From).ToList());
    }

    [HttpGet("occurrences", Name = "GetMaintenanceOccurrences")]
    [ProducesResponseType<IReadOnlyList<MaintenanceOccurrenceResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOccurrences(
        Guid orgId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var rows = await bus.InvokeAsync<IReadOnlyList<MaintenanceOccurrenceRow>>(
            new GetMaintenanceOccurrencesQuery(orgId, from, to), cancellationToken);
        return Ok(rows.Select(MaintenanceOccurrenceResponse.From).ToList());
    }

    [HttpPost(Name = "CreateMaintenanceWindow")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        Guid orgId,
        CreateMaintenanceWindowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMaintenanceWindowCommand(
            orgId,
            request.Name,
            request.Description,
            request.Schedule,
            request.MonitorIds,
            request.SuppressNotifications,
            request.ExcludeFromSla);
        var id = await bus.InvokeAsync<Guid>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPut("{id:guid}/content", Name = "RenameMaintenanceWindow")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Rename(
        Guid orgId,
        Guid id,
        RenameMaintenanceWindowRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RenameMaintenanceWindowCommand(
            orgId, id, request.Name, request.Description, request.ApplyToPastOccurrences);
        await bus.InvokeAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/schedule", Name = "UpdateMaintenanceWindowSchedule")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSchedule(
        Guid orgId,
        Guid id,
        MaintenanceScheduleInput request,
        CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new UpdateMaintenanceWindowScheduleCommand(orgId, id, request), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/policy", Name = "UpdateMaintenanceWindowPolicy")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePolicy(
        Guid orgId,
        Guid id,
        UpdateMaintenanceWindowPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMaintenanceWindowPolicyCommand(
            orgId, id, request.SuppressNotifications, request.ExcludeFromSla);
        await bus.InvokeAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/monitors", Name = "SetMaintenanceWindowMonitors")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetMonitors(
        Guid orgId,
        Guid id,
        SetMaintenanceWindowMonitorsRequest request,
        CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new SetMaintenanceWindowMonitorsCommand(orgId, id, request.MonitorIds), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = "DeleteMaintenanceWindow")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid orgId, Guid id, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new DeleteMaintenanceWindowCommand(orgId, id), cancellationToken);
        return NoContent();
    }
}
