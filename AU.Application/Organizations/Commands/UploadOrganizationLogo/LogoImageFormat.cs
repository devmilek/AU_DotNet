namespace AU.Application.Organizations.Commands.UploadOrganizationLogo;

public sealed record LogoImageFormat(string Extension, string ContentType)
{
    private static readonly LogoImageFormat Png = new(".png", "image/png");
    private static readonly LogoImageFormat Jpeg = new(".jpg", "image/jpeg");
    private static readonly LogoImageFormat WebP = new(".webp", "image/webp");

    public static async Task<LogoImageFormat?> DetectAsync(Stream content, CancellationToken ct)
    {
        if (!content.CanSeek)
            throw new ArgumentException("Logo content stream must be seekable.", nameof(content));

        var header = new byte[12];
        content.Position = 0;
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        content.Position = 0;

        return Detect(header.AsSpan(0, read));
    }

    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];

    private static LogoImageFormat? Detect(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith(PngSignature))
            return Png;

        if (header.StartsWith(JpegSignature))
            return Jpeg;

        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
            return WebP;

        return null;
    }
}
