namespace US.Application.Abstractions;

public interface IUserLookup
{
    Task<Guid?> FindIdByEmailAsync(string email);

    /// <summary>Nazwa do pokazania w mailu — DisplayName, a w razie braku adres email.</summary>
    Task<string?> FindDisplayNameByIdAsync(Guid userId);
}
