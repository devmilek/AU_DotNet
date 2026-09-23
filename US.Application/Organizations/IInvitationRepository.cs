using US.Domain.Entities;

namespace US.Application.Organizations;

public interface IInvitationRepository
{
    Task AddAsync(Invitation invitation);
    Task<Invitation?> GetPendingByEmailAsync(Guid organizationId, string normalizedEmail);
    Task<Invitation?> GetByTokenHashAsync(string tokenHash);
    Task<IReadOnlyList<Invitation>> GetPendingAsync(Guid organizationId);
}