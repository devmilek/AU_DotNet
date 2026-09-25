using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using US.Api.Controllers.MaintenanceWindows.Requests;
using US.Application.MaintenanceWindows;
using US.Application.MaintenanceWindows.Commands.CreateMaintenanceWindow;
using US.Application.MaintenanceWindows.Commands.DeleteMaintenanceWindow;
using US.Application.MaintenanceWindows.Commands.RenameMaintenanceWindow;
using US.Application.MaintenanceWindows.Commands.SetMaintenanceWindowMonitors;
using US.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowPolicy;
using US.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowSchedule;
using Wolverine;

namespace US.Api.Controllers;

[ApiController]
[Tags("MaintenanceWindows")]
[Produces("application/json")]
[Route("api/{orgId:guid}/maintenance-windows")]
[Authorize(Policy = OrgPolicies.Admin)]
public class MaintenanceWindowsController(IMessageBus bus) : ControllerBase
{
    [HttpPost(Name = "CreateMaintenanceWindow")]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid orgId, Guid id, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new DeleteMaintenanceWindowCommand(orgId, id), cancellationToken);
        return NoContent();
    }
}
