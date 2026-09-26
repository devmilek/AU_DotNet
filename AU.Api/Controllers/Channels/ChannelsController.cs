using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AU.Api.Controllers.Channels.Requests;
using AU.Api.Controllers.Channels.Responses;
using AU.Application.Channels;
using AU.Application.Channels.Commands.CreateNotificationChannel;
using AU.Application.Channels.Commands.SetChannelMonitors;
using AU.Application.Channels.Queries.GetNotificationChannel;
using AU.Application.Channels.Queries.GetNotificationChannels;
using Wolverine;

namespace AU.Api.Controllers;

[ApiController]
[Tags("Channels")]
[Produces("application/json")]
[Route("api/{orgId:guid}/channels")]
[Authorize(Policy = OrgPolicies.Member)]
public class ChannelsController(IMessageBus bus) : ControllerBase
{
    [HttpGet(Name = "GetChannels")]
    [ProducesResponseType<IReadOnlyList<NotificationChannelListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid orgId, CancellationToken cancellationToken)
    {
        var rows = await bus.InvokeAsync<IReadOnlyList<ChannelListRow>>(
            new GetNotificationChannelsQuery(orgId), cancellationToken);
        return Ok(rows.Select(NotificationChannelListItemResponse.From).ToList());
    }

    [HttpGet("{id:guid}", Name = "GetChannel")]
    [ProducesResponseType<NotificationChannelResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid orgId, Guid id, CancellationToken cancellationToken)
    {
        var details = await bus.InvokeAsync<NotificationChannelDetails>(
            new GetNotificationChannelQuery(orgId, id), cancellationToken);
        return Ok(NotificationChannelResponse.From(details));
    }

    [HttpPost(Name = "CreateChannel")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> CreateChannel(
        Guid orgId,
        CreateNotificationChannelRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateNotificationChannelCommand(
            orgId, request.Name, request.Type, request.Email, request.MonitorIds);
        var id = await bus.InvokeAsync<Guid>(command, cancellationToken);

        return CreatedAtAction(nameof(Get), new { orgId, id }, id);
    }

    [HttpPut("{id:guid}/monitors", Name = "SetChannelMonitors")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> SetMonitors(
        Guid orgId,
        Guid id,
        SetChannelMonitorsRequest request,
        CancellationToken cancellationToken)
    {
        await bus.InvokeAsync(new SetChannelMonitorsCommand(orgId, id, request.MonitorIds), cancellationToken);
        return NoContent();
    }
}
