using System.Text.RegularExpressions;

namespace AU.Domain;

public static partial class OrganizationSlug
{
    public const int MinLength = 3;
    public const int MaxLength = 50;
    public const string Pattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    private const int SuffixLength = 6;

    private static readonly HashSet<string> Reserved =
    [
        "api", "app", "admin", "auth", "new", "settings", "status", "files", "static", "public",
        "organizations", "sign-in", "sign-up", "sign-out", "invite", "confirm-email",
        "forgot-password", "reset-password", "favicon-ico", "robots-txt", "sitemap-xml"
    ];

    public static bool IsValid(string slug) =>
        slug.Length is >= MinLength and <= MaxLength && FormatRegex().IsMatch(slug);

    public static bool IsReserved(string slug) => Reserved.Contains(slug);

    public static bool IsUsable(string slug) => IsValid(slug) && !IsReserved(slug);

    public static string FromName(string name)
    {
        var slug = Slugger.Create(name);

        if (slug.Length > MaxLength)
            slug = slug[..MaxLength].TrimEnd('-');

        return slug.Length >= MinLength && FormatRegex().IsMatch(slug) ? slug : "org";
    }

    public static string WithRandomSuffix(string slug) =>
        $"{slug}-{Guid.NewGuid().ToString("N")[..SuffixLength]}";

    [GeneratedRegex(Pattern)]
    private static partial Regex FormatRegex();
}
