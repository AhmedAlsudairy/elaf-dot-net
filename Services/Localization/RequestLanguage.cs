using System.Security.Claims;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Localization;

/// <summary>The language content is read in for the current request.</summary>
public interface IRequestLanguage
{
    string Code { get; }
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
