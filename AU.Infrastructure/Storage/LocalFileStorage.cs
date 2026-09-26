using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using AU.Application.Abstractions;

namespace AU.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    public const string RequestPath = "/api/files";

    private readonly string _publicBaseUrl;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options, IHostEnvironment environment)
    {
        RootPath = Path.GetFullPath(options.Value.RootPath, environment.ContentRootPath);
        _publicBaseUrl = options.Value.PublicBaseUrl.TrimEnd('/');

        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);

        await content.CopyToAsync(file, ct);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        File.Delete(ResolvePath(key));
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => $"{_publicBaseUrl}/{key}";

    private string ResolvePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("File key is required.", nameof(key));

        var path = Path.GetFullPath(Path.Combine(RootPath, key));

        if (!path.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("File key must point inside the storage root.", nameof(key));

        return path;
    }
}
