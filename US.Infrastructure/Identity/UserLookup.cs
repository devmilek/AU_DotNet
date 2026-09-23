using Microsoft.AspNetCore.Identity;
using US.Application.Abstractions;

namespace US.Infrastructure.Identity;

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
}
