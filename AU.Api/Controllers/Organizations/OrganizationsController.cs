using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AU.Api.Controllers.Organizations.Requests;
using AU.Api.RateLimiting;
using AU.Application.Organizations.Commands.ChangeMemberRole;
using AU.Application.Organizations.Commands.CreateOrganization;
using AU.Application.Organizations.Commands.DeleteOrganization;
using AU.Application.Organizations.Commands.InviteMemberCommand;
using AU.Application.Organizations.Commands.RemoveMember;
using AU.Application.Organizations.Commands.RemoveOrganizationLogo;
using AU.Application.Organizations.Commands.RevokeInvitation;
using AU.Application.Organizations.Commands.UpdateOrganization;
using AU.Application.Organizations.Commands.UploadOrganizationLogo;
using AU.Application.Organizations.Queries.GetInvitations;
using AU.Application.Organizations.Queries.GetMembers;
using AU.Application.Organizations.Queries.GetMyOrganizations;
using AU.Application.Organizations.Queries.GetOrganization;
using AU.Application.Organizations.Queries.GetSlugAvailability;
using Wolverine;

namespace AU.Api.Controllers;

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

    [HttpGet("slug-availability", Name = "GetSlugAvailability")]
    [ProducesResponseType<SlugAvailability>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SlugAvailability>> GetSlugAvailability([FromQuery] string slug, CancellationToken ct)
    {
        return Ok(await bus.InvokeAsync<SlugAvailability>(new GetSlugAvailabilityQuery(slug), ct));
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

    [HttpGet("{orgId:guid}", Name = "GetOrganization")]
    [ProducesResponseType<OrganizationDetails>(StatusCodes.Status200OK)]
    [Authorize(Policy = OrgPolicies.Member)]
    public async Task<IActionResult> Get(Guid orgId, CancellationToken ct)
    {
        return Ok(await bus.InvokeAsync<OrganizationDetails>(new GetOrganizationQuery(orgId), ct));
    }

    [HttpPut("{orgId:guid}", Name = "UpdateOrganization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> Update(Guid orgId, UpdateOrganizationRequest request, CancellationToken ct)
    {
        await bus.InvokeAsync(new UpdateOrganizationCommand(orgId, request.Name), ct);
        return NoContent();
    }

    [HttpPut("{orgId:guid}/logo", Name = "UploadOrganizationLogo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<OrganizationLogoResponse>(StatusCodes.Status200OK)]
    [Authorize(Policy = OrgPolicies.Admin)]
    [RequestSizeLimit(UploadOrganizationLogoValidator.MaxSizeBytes + 64 * 1024)]
    public async Task<ActionResult<OrganizationLogoResponse>> UploadLogo(
        Guid orgId, IFormFile file, CancellationToken ct)
    {
        await using var content = file.OpenReadStream();
        var result = await bus.InvokeAsync<OrganizationLogoResponse>(
            new UploadOrganizationLogoCommand(orgId, content), ct);
        return Ok(result);
    }

    [HttpDelete("{orgId:guid}/logo", Name = "RemoveOrganizationLogo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> RemoveLogo(Guid orgId, CancellationToken ct)
    {
        await bus.InvokeAsync(new RemoveOrganizationLogoCommand(orgId), ct);
        return NoContent();
    }

    [HttpDelete("{orgId:guid}", Name = "DeleteOrganization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Owner)]
    public async Task<IActionResult> Delete(Guid orgId, [FromBody] DeleteOrganizationRequest request, CancellationToken ct)
    {
        await bus.InvokeAsync(new DeleteOrganizationCommand(orgId, request.ConfirmationName), ct);
        return NoContent();
    }

    [HttpGet("{orgId:guid}/members", Name = "GetMembers")]
    [ProducesResponseType<IReadOnlyList<MemberRow>>(StatusCodes.Status200OK)]
    [Authorize(Policy = OrgPolicies.Member)]
    public async Task<IActionResult> GetMembers(Guid orgId, CancellationToken ct)
    {
        return Ok(await bus.InvokeAsync<IReadOnlyList<MemberRow>>(new GetMembersQuery(orgId), ct));
    }

    [HttpPut("{orgId:guid}/members/{userId:guid}/role", Name = "ChangeMemberRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> ChangeMemberRole(
        Guid orgId, Guid userId, ChangeMemberRoleRequest request, CancellationToken ct)
    {
        await bus.InvokeAsync(new ChangeMemberRoleCommand(orgId, userId, request.Role), ct);
        return NoContent();
    }

    [HttpDelete("{orgId:guid}/members/{userId:guid}", Name = "RemoveMember")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Member)]
    public async Task<IActionResult> RemoveMember(Guid orgId, Guid userId, CancellationToken ct)
    {
        await bus.InvokeAsync(new RemoveMemberCommand(orgId, userId), ct);
        return NoContent();
    }

    [HttpGet("{orgId:guid}/invitations", Name = "GetInvitations")]
    [ProducesResponseType<IReadOnlyList<PendingInvitationRow>>(StatusCodes.Status200OK)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> GetInvitations(Guid orgId, CancellationToken ct)
    {
        return Ok(await bus.InvokeAsync<IReadOnlyList<PendingInvitationRow>>(new GetInvitationsQuery(orgId), ct));
    }

    [HttpDelete("{orgId:guid}/invitations/{invitationId:guid}", Name = "RevokeInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = OrgPolicies.Admin)]
    public async Task<IActionResult> RevokeInvitation(Guid orgId, Guid invitationId, CancellationToken ct)
    {
        await bus.InvokeAsync(new RevokeInvitationCommand(orgId, invitationId), ct);
        return NoContent();
    }
}
