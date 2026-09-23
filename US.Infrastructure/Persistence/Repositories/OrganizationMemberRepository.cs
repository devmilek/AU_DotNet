using Microsoft.EntityFrameworkCore;
using US.Application.Organizations;
using US.Domain.Entities;
using US.Domain.Enums;

namespace US.Infrastructure.Persistence.Repositories;

public class OrganizationMemberRepository(AppDbContext context) : IOrganizationMemberRepository
{
    public async Task<OrganizationRole?> GetRoleAsync(Guid organizationId, Guid userId)
    {
        return await context.Set<OrganizationMember>()
            .Where(m => m.OrganizationId == organizationId && m.UserId == userId)
            .Select(m => (OrganizationRole?)m.Role)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<OrganizationMember>> GetMembersAsync(Guid organizationId)
    {
        return await context.Set<OrganizationMember>()
            .Where(m => m.OrganizationId == organizationId)
            .ToListAsync();
    }
}