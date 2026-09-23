using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using US.Api.Controllers.Channels.Requests;
using US.Application.Channels.Commands.CreateNotificationChannel;
using Wolverine;

namespace US.Api.Controllers;

[ApiController]
[Tags("Channels")]
[Produces("application/json")]
[Route("api/{orgId:guid}/channels")]
[Authorize(Policy = OrgPolicies.Member)]
public class ChannelsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetChannel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Get(Guid orgId, Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost(Name = "CreateChannel")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChannel(Guid orgId, CreateNotificationChannelRequest request)
    {
        var command = new CreateNotificationChannelCommand(orgId, request.Name, request.Type, request.EmailTo);
        var id = await bus.InvokeAsync<Guid>(command);

        return CreatedAtAction(nameof(Get), new { orgId, id }, id);
    }
}
