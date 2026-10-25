using System.Globalization;
using System.Text;

namespace Munaqasat.Web.Services.Common;

public static class Slug
{
    public static string From(string value, int maxLength = 80)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var lastWasHyphen = false;

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsAsciiLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                lastWasHyphen = true;
            }
        }

        return builder.ToString().Trim('-') is { Length: > 0 } slug
            ? slug[..Math.Min(slug.Length, maxLength)].TrimEnd('-')
            : "";
    }
}
