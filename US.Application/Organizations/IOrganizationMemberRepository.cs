using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Application.Organizations;

public interface IOrganizationMemberRepository
{
    Task<OrganizationRole?> GetRoleAsync(Guid organizationId, Guid userId);
    Task<IReadOnlyList<OrganizationMember>> GetMembersAsync(Guid organizationId);
}