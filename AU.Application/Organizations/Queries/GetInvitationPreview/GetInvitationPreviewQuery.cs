using System.Text.Json.Serialization;
using AU.Application.Abstractions;
using AU.Application.Exceptions;
using AU.Domain.Enums;

namespace AU.Application.Organizations.Queries.GetInvitationPreview;

public sealed record GetInvitationPreviewQuery(string Token);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvitationState
{
    Pending,
    Accepted,
    Revoked,
    Expired
}

public sealed record InvitationPreview(
    string OrganizationName,
    string Email,
    OrganizationRole Role,
    string? InvitedByName,
    DateTimeOffset ExpiresAt,
    InvitationState State);

public sealed class GetInvitationPreviewHandler
{
    public async Task<InvitationPreview> Handle(
        GetInvitationPreviewQuery query,
        IInvitationRepository invitations,
        IOrganizationRepository organizations,
        IInvitationTokenService tokenService,
        IUserLookup userLookup,
        TimeProvider timeProvider)
    {
        var invitation = await invitations.GetByTokenHashAsync(tokenService.Hash(query.Token))
                         ?? throw new NotFoundException("Invitation", Guid.Empty);

        var organization = await organizations.GetAsync(invitation.OrganizationId)
                           ?? throw new NotFoundException("Organization", invitation.OrganizationId);

        var now = timeProvider.GetUtcNow();
        var state = invitation switch
        {
            { AcceptedAt: not null } => InvitationState.Accepted,
            { RevokedAt: not null } => InvitationState.Revoked,
            _ when invitation.ExpiresAt <= now => InvitationState.Expired,
            _ => InvitationState.Pending
        };

        return new InvitationPreview(
            organization.Name,
            invitation.Email,
            invitation.Role,
            await userLookup.FindDisplayNameByIdAsync(invitation.InvitedByUserId),
            invitation.ExpiresAt,
            state);
    }
}
