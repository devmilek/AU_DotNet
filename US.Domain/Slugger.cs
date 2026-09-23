using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace US.Domain;

public static partial class Slugger
{
    public static string Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty.", nameof(value));

        var normalized = value
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else
            {
                builder.Append('-');
            }
        }

        var slug = MultipleDashesRegex()
            .Replace(builder.ToString(), "-")
            .Trim('-');

        return slug;
    }

    [GeneratedRegex("-+")]
    private static partial Regex MultipleDashesRegex();
}