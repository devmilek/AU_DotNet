namespace AU.Application.Exceptions;

/// <summary>
/// Zasób nie istnieje albo nie należy do organizacji z żądania.
/// Mapowane w API na 404 — świadomie nie 403, żeby nie ujawniać istnienia
/// zasobów z innych organizacji.
/// </summary>
public sealed class NotFoundException(string resource, Guid id)
    : Exception($"{resource} o id {id} nie został znaleziony.")
{
    public string Resource { get; } = resource;
    public Guid Id { get; } = id;
}
