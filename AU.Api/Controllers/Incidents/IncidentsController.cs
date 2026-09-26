using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AU.Api.Controllers.Incidents.Requests;
using AU.Application.Common;
using AU.Application.Incidents;
using AU.Application.Incidents.Commands.AcknowledgeIncident;
using AU.Application.Incidents.Commands.DeleteIncident;
using AU.Application.Incidents.Commands.UpdateIncident;
using AU.Application.Incidents.Queries.GetIncident;
using AU.Application.Incidents.Queries.GetIncidents;
using Wolverine;

namespace AU.Api.Controllers;

[ApiController]
[Tags("Incidents")]
[Produces("application/json")]
[Route("api/{orgId:guid}/incidents")]
[Authorize(Policy = OrgPolicies.Member)]
public class IncidentsController(IMessageBus bus) : ControllerBase
{
    [HttpGet(Name = "GetIncidents")]
    [ProducesResponseType<PagedResult<IncidentResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        Guid orgId,
        [FromQuery] IncidentStatusFilter status = IncidentStatusFilter.All,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? monitorId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await bus.InvokeAsync<PagedResult<IncidentResponse>>(
            new GetIncidentsQuery(orgId, status, page, pageSize, monitorId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetIncident")]
    [ProducesResponseType<IncidentResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid orgId, Guid id, CancellationToken cancellationToken)
    {
        return Ok(await bus.InvokeAsync<IncidentResponse>(new GetIncidentQuery(orgId, id), cancellationToken));
    }

    [HttpPost("{id:guid}/acknowledge", Name = "AcknowledgeIncident")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Acknowledge(Guid orgId, Guid id, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new AcknowledgeIncidentCommand(orgId, id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}", Name = "UpdateIncident")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid orgId, Guid id, UpdateIncidentRequest request, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new UpdateIncidentCommand(orgId, id, request.Name, request.Cause), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = "DeleteIncident")]
    [Authorize(Policy = OrgPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid orgId, Guid id, CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new DeleteIncidentCommand(orgId, id), cancellationToken);
        return NoContent();
    }
}
