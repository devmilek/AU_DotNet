namespace US.Infrastructure.Notifications;

/// <summary>Obrazy osadzane w mailach jako załączniki inline (<c>cid:</c>) — działają też tam, gdzie zdalne obrazy są blokowane.</summary>
internal static class EmailAssets
{
    public const string LogoContentId = "logo";

    public static readonly Lazy<byte[]> Logo = new(() =>
    {
        const string resourceName = "US.Infrastructure.Notifications.Templates.assets.logo.png";
        using var stream = typeof(EmailAssets).Assembly.GetManifestResourceStream(resourceName)
                           ?? throw new FileNotFoundException($"Nie znaleziono zasobu: {resourceName}");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    });
}
