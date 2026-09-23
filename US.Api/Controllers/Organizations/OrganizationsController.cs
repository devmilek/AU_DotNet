using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using US.Api.Controllers.Organizations.Requests;
using US.Api.RateLimiting;
using US.Application.Organizations.Commands.CreateOrganization;
using US.Application.Organizations.Commands.InviteMemberCommand;
using US.Application.Organizations.Queries.GetMyOrganizations;
using Wolverine;

namespace US.Api.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OrganizationResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OrganizationResponse>> Create(CreateOrganizationCommand command)
    {
        var result = await bus.InvokeAsync<OrganizationResponse>(command);
        return CreatedAtAction(nameof(GetMine), new { }, result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrganizationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganizationResponse>>> GetMine()
    {
        var result = await bus.InvokeAsync<MyOrganizationsResponse>(new GetMyOrganizationsQuery());
        return Ok(result.Organizations);
    }

    [HttpPost("{orgId:guid}/invitations", Name = "InviteMember")]
    [ProducesResponseType<InvitationResponse>(StatusCodes.Status200OK)]
    [Authorize(Policy = OrgPolicies.Admin)]
    [EnableRateLimiting(RateLimitingSetup.InvitePolicy)]
    public async Task<ActionResult<InvitationResponse>> Invite(
        Guid orgId, InviteMemberRequest request, CancellationToken ct)
    {
        var command = new InviteMemberCommand(orgId, request.Email, request.Role);
        return Ok(await bus.InvokeAsync<InvitationResponse>(command, ct));
    }
}
