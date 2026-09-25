using Microsoft.EntityFrameworkCore;
using US.Application.Organizations;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(AppDbContext context) : IOrganizationRepository
{
    public async Task AddAsync(Organization organization)
    {
        await context.Organizations.AddAsync(organization);
    }

    public async Task<Organization?> GetAsync(Guid organizationId)
    {
        return await context.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId);
    }

    public async Task<Organization?> GetWithMembersAsync(Guid organizationId, CancellationToken ct = default)
    {
        return await context.Organizations
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Id == organizationId, ct);
    }

    public async Task DeleteAsync(Organization organization, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            await context.MaintenanceOccurrences
                .Where(o => context.MaintenanceWindows.Any(w =>
                    w.Id == o.MaintenanceWindowId && w.OrganizationId == organization.Id))
                .ExecuteDeleteAsync(ct);

            context.Organizations.Remove(organization);
            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        });
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await context.Organizations.Where(x => x.Slug == slug).AsNoTracking().AnyAsync();
    }

    public async Task<IReadOnlyList<Organization>> GetForUserAsync(Guid userId)
    {
        return await context.Organizations
            .Where(o => o.Members.Any(m => m.UserId == userId))
            .Include(o => o.Members)
            .AsNoTracking()
            .ToListAsync();    
    }
}