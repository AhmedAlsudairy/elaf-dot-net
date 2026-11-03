using System.Security.Claims;
using Munaqasat.Web.Domain;

namespace Munaqasat.Web.Services.Common;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Identity user id (stored in <c>user_profiles.clerk_user_id</c>).</summary>
    string? AuthUserId { get; }

    /// <summary><c>user_profiles.id</c>.</summary>
    string? ProfileId { get; }

    /// <summary><c>companies.id</c> of the user's company, if any.</summary>
    string? CompanyId { get; }

    string? Name { get; }

    /// <summary>Frontend role key: client, contractor, individual or admin.</summary>
    string? RoleKey { get; }

    bool IsAdministrator { get; }
}

public sealed class HttpContextCurrentUser : ICurrentUser
{
    public const string ProfileIdClaim = "munaqasat:profile_id";
    public const string CompanyIdClaim = "munaqasat:company_id";
    public const string RoleKeyClaim = "munaqasat:role";
    public const string LanguageClaim = "munaqasat:lang";
    public const string DisplayNameClaim = "munaqasat:name";

    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string? AuthUserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? ProfileId => NullIfEmpty(Principal?.FindFirstValue(ProfileIdClaim));
    public string? CompanyId => NullIfEmpty(Principal?.FindFirstValue(CompanyIdClaim));
    public string? Name => NullIfEmpty(Principal?.FindFirstValue(DisplayNameClaim));
    public string? RoleKey => NullIfEmpty(Principal?.FindFirstValue(RoleKeyClaim));
    public bool IsAdministrator => Principal?.IsInRole(Roles.Administrator) ?? false;

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;
}
