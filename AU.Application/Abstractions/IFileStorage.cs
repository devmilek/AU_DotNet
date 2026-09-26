namespace AU.Application.Abstractions;

public interface IFileStorage
{
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
    string GetPublicUrl(string key);
}
