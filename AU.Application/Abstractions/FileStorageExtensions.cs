using Microsoft.Extensions.Logging;

namespace AU.Application.Abstractions;

public static class FileStorageExtensions
{
    public static string? GetPublicUrlOrDefault(this IFileStorage storage, string? key) =>
        key is null ? null : storage.GetPublicUrl(key);

    public static async Task TryDeleteAsync(this IFileStorage storage, string key, ILogger logger)
    {
        try
        {
            await storage.DeleteAsync(key, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to delete stored file {FileKey}", key);
        }
    }
}
