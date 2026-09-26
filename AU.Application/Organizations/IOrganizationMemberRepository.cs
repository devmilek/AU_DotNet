using AU.Domain.Entities;
using AU.Domain.Enums;

namespace AU.Application.Organizations;

public interface IOrganizationMemberRepository
{
    Task<OrganizationRole?> GetRoleAsync(Guid organizationId, Guid userId);
    Task<IReadOnlyList<OrganizationMember>> GetMembersAsync(Guid organizationId);
}