namespace AU.Application.Abstractions;

public interface IUserLookup
{
    Task<Guid?> FindIdByEmailAsync(string email);

    /// <summary>Nazwa do pokazania w mailu — DisplayName, a w razie braku adres email.</summary>
    Task<string?> FindDisplayNameByIdAsync(Guid userId);

    Task<string?> FindEmailByIdAsync(Guid userId);

    Task<IReadOnlyDictionary<Guid, UserSummary>> GetUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct = default);
}

public sealed record UserSummary(Guid Id, string Email, string? DisplayName);
