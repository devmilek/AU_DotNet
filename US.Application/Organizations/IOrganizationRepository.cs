using US.Domain.Entities;

namespace US.Application.Organizations;

public interface IOrganizationRepository
{
    Task AddAsync(Organization organization);
    Task<Organization?> GetAsync(Guid organizationId);
    Task<Organization?> GetWithMembersAsync(Guid organizationId, CancellationToken ct = default);
    Task DeleteAsync(Organization organization, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug);
    Task<IReadOnlyList<Organization>> GetForUserAsync(Guid userId);
}