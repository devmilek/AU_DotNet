namespace AU.Infrastructure.Storage;

public sealed class LocalFileStorageOptions
{
    public const string SectionName = "Storage:Local";

    public string RootPath { get; set; } = "storage";
    public string PublicBaseUrl { get; set; } = LocalFileStorage.RequestPath;
}
