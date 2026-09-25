using US.Application.Abstractions;
using US.Domain.Enums;

namespace US.Application.Organizations.Queries.GetMembers;

public sealed record GetMembersQuery(Guid OrganizationId);

public sealed record MemberRow(
    Guid UserId,
    string Email,
    string? DisplayName,
    OrganizationRole Role,
    DateTimeOffset JoinedAt,
    bool IsCurrentUser);

public sealed class GetMembersHandler
{
    public async Task<IReadOnlyList<MemberRow>> Handle(
        GetMembersQuery query,
        IOrganizationMemberRepository members,
        IUserLookup userLookup,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var memberships = await members.GetMembersAsync(query.OrganizationId);
        var users = await userLookup.GetUsersAsync(memberships.Select(m => m.UserId).ToList(), cancellationToken);

        return memberships
            .Select(m =>
            {
                users.TryGetValue(m.UserId, out var user);
                return new MemberRow(
                    m.UserId,
                    user?.Email ?? "",
                    user?.DisplayName,
                    m.Role,
                    m.JoinedAt,
                    m.UserId == currentUser.UserId);
            })
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.DisplayName ?? m.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
