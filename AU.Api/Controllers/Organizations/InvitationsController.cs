using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AU.Api.RateLimiting;
using AU.Application.Organizations.Commands.AcceptInvitation;
using AU.Application.Organizations.Commands.CreateOrganization;
using AU.Application.Organizations.Queries.GetInvitationPreview;
using Wolverine;

namespace AU.Api.Controllers;

[ApiController]
[Tags("Invitations")]
[Produces("application/json")]
[Route("api/invitations")]
public class InvitationsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{token}", Name = "GetInvitationPreview")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    [ProducesResponseType<InvitationPreview>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview(string token, CancellationToken ct)
    {
        return Ok(await bus.InvokeAsync<InvitationPreview>(new GetInvitationPreviewQuery(token), ct));
    }

    [HttpPost("{token}/accept", Name = "AcceptInvitation")]
    [ProducesResponseType<OrganizationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Accept(string token, CancellationToken ct)
    {
        return Ok(await bus.InvokeAsync<OrganizationResponse>(new AcceptInvitationCommand(token), ct));
    }
}
