using US.Domain.Entities;

namespace US.Application.Organizations;

public interface IOrganizationRepository
{
    Task AddAsync(Organization organization);
    Task<Organization?> GetAsync(Guid organizationId);
    Task<bool> SlugExistsAsync(string slug);
    Task<IReadOnlyList<Organization>> GetForUserAsync(Guid userId);
}