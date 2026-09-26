using Microsoft.EntityFrameworkCore;
using AU.Application.Organizations;
using AU.Domain.Entities;

namespace AU.Infrastructure.Persistence.Repositories;

public sealed class InvitationRepository(AppDbContext context) : IInvitationRepository
{
    public async Task AddAsync(Invitation invitation)
    {
        await context.Invitations.AddAsync(invitation);
    }

    public async Task<Invitation?> GetPendingByEmailAsync(Guid organizationId, string normalizedEmail)
    {
        var now = DateTimeOffset.UtcNow;

        // śledzone — wywołujący może chcieć zaproszenie odwołać
        return await context.Invitations
            .Where(i => i.OrganizationId == organizationId && i.Email == normalizedEmail)
            .Where(i => i.AcceptedAt == null && i.RevokedAt == null && i.ExpiresAt > now)
            .FirstOrDefaultAsync();
    }

    public async Task<Invitation?> GetByTokenHashAsync(string tokenHash)
    {
        // bez filtra na pending — wywołujący rozróżnia wygasłe/odwołane/zużyte
        return await context.Invitations
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash);
    }

    public async Task<Invitation?> GetAsync(Guid organizationId, Guid invitationId, CancellationToken ct = default)
    {
        return await context.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.OrganizationId == organizationId, ct);
    }

    public async Task<IReadOnlyList<Invitation>> GetPendingAsync(Guid organizationId)
    {
        var now = DateTimeOffset.UtcNow;

        return await context.Invitations
            .Where(i => i.OrganizationId == organizationId)
            .Where(i => i.AcceptedAt == null && i.RevokedAt == null && i.ExpiresAt > now)
            .OrderByDescending(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
