using US.Application.Abstractions;
using US.Domain.Enums;

namespace US.Application.Organizations.Queries.GetInvitations;

public sealed record GetInvitationsQuery(Guid OrganizationId);

public sealed record PendingInvitationRow(
    Guid Id,
    string Email,
    OrganizationRole Role,
    string? InvitedByName,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public sealed class GetInvitationsHandler
{
    public async Task<IReadOnlyList<PendingInvitationRow>> Handle(
        GetInvitationsQuery query,
        IInvitationRepository invitations,
        IUserLookup userLookup,
        CancellationToken cancellationToken)
    {
        var pending = await invitations.GetPendingAsync(query.OrganizationId);
        var inviters = await userLookup.GetUsersAsync(
            pending.Select(i => i.InvitedByUserId).Distinct().ToList(), cancellationToken);

        return pending
            .Select(i =>
            {
                inviters.TryGetValue(i.InvitedByUserId, out var inviter);
                return new PendingInvitationRow(
                    i.Id,
                    i.Email,
                    i.Role,
                    inviter?.DisplayName ?? inviter?.Email,
                    i.CreatedAt,
                    i.ExpiresAt);
            })
            .ToList();
    }
}
