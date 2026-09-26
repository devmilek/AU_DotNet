using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AU.Application.Abstractions;

namespace AU.Infrastructure.Identity;

public sealed class UserLookup(UserManager<ApplicationUser> userManager) : IUserLookup
{
    public async Task<Guid?> FindIdByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        return user?.Id;
    }

    public async Task<string?> FindDisplayNameByIdAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user is null ? null : user.DisplayName ?? user.Email;
    }

    public async Task<string?> FindEmailByIdAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user?.Email;
    }

    public async Task<IReadOnlyDictionary<Guid, UserSummary>> GetUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct = default)
    {
        return await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserSummary(u.Id, u.Email ?? "", u.DisplayName))
            .ToDictionaryAsync(u => u.Id, ct);
    }
}
