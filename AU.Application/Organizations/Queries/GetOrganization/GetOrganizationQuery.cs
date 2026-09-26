using AU.Application.Abstractions;
using AU.Application.Exceptions;
using AU.Domain.Enums;

namespace AU.Application.Organizations.Queries.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrganizationId);

public sealed record OrganizationDetails(
    Guid Id,
    string Name,
    string Slug,
    DateTimeOffset CreatedAt,
    int MemberCount,
    OrganizationRole CurrentUserRole);

public sealed class GetOrganizationHandler
{
    public async Task<OrganizationDetails> Handle(
        GetOrganizationQuery query,
        IOrganizationRepository organizations,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var organization = await organizations.GetWithMembersAsync(query.OrganizationId, cancellationToken)
                           ?? throw new NotFoundException("Organization", query.OrganizationId);

        var member = organization.FindMember(currentUser.UserId)
                     ?? throw new ForbiddenException("You are not a member of this organization.");

        return new OrganizationDetails(
            organization.Id,
            organization.Name,
            organization.Slug,
            organization.CreatedAt,
            organization.Members.Count,
            member.Role);
    }
}
