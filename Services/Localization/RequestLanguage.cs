using System.Security.Claims;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Localization;

/// <summary>The language content is read in for the current request.</summary>
public interface IRequestLanguage
{
    string Code { get; }

    /// <summary>True when the request language is written right to left (Arabic).</summary>
    bool IsRtl { get; }

    /// <summary>Interface text: the Arabic for <paramref name="english"/> when the request is in Arabic, else the English.</summary>
    string T(string? english);

    /// <summary><see cref="T(string?)"/> then <see cref="string.Format(string, object?[])"/>.</summary>
    string T(string english, params object?[] args);
}

/// <summary>
/// Resolution order: <c>?lang=</c>, the <c>munaqasat_lang</c> cookie, the signed-in user's
/// <c>preferred_language</c>, then English.
/// </summary>
public sealed class RequestLanguage : IRequestLanguage
{
    public const string CookieName = "munaqasat_lang";
    public static readonly string[] Supported = { "en", "ar" };

    private readonly Lazy<string> _code;

    public RequestLanguage(IHttpContextAccessor accessor) => _code = new Lazy<string>(() => Resolve(accessor.HttpContext));

    public string Code => _code.Value;

    public bool IsRtl => Code == "ar";

    public string T(string? english) => UiText.Translate(english, Code);

    public string T(string english, params object?[] args) => string.Format(Culture(Code), T(english), args);

    /// <summary>
    /// Formatting culture for a language. Arabic uses Arabic month names but keeps Western digits and
    /// "." / "," separators, so amounts read the same in both languages.
    /// </summary>
    public static System.Globalization.CultureInfo Culture(string code)
    {
        if (code != "ar") return System.Globalization.CultureInfo.InvariantCulture;
        var culture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.GetCultureInfo("ar-OM").Clone();
        culture.NumberFormat = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
        culture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
        return culture;
    }

    private static string Resolve(HttpContext? http)
    {
        if (http is null) return Translations.Fallback;

        var candidates = new[]
        {
            http.Request.Query["lang"].ToString(),
            http.Request.Cookies[CookieName],
            http.User.FindFirstValue(HttpContextCurrentUser.LanguageClaim)
        };

        return candidates.FirstOrDefault(c => !string.IsNullOrEmpty(c) && Supported.Contains(c)) ?? Translations.Fallback;
    }
}

public static class Translations
{
    public const string Fallback = "en";

    /// <summary>Fallback rule from the ERD: requested language, record default, then English, then anything.</summary>
    public static T? Pick<T>(IEnumerable<T> rows, string requested, string? recordDefault = null) where T : class, ITranslation
    {
        var list = rows as IReadOnlyCollection<T> ?? rows.ToList();
        return list.FirstOrDefault(r => r.LanguageCode == requested)
            ?? (recordDefault is null ? null : list.FirstOrDefault(r => r.LanguageCode == recordDefault))
            ?? list.FirstOrDefault(r => r.LanguageCode == Fallback)
            ?? list.FirstOrDefault();
    }
}
