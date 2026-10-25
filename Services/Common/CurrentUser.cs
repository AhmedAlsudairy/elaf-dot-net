using System.Security.Claims;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Services.Common;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }

    Guid? CompanyId { get; }

    bool IsAdministrator { get; }
    bool IsInRole(string role);
}

public sealed class HttpContextCurrentUser : ICurrentUser
{
    public const string CompanyIdClaimType = "munaqasat:company_id";

    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public Guid? CompanyId =>
        Guid.TryParse(Principal?.FindFirstValue(CompanyIdClaimType), out var id) ? id : null;

    public bool IsAdministrator => IsInRole(AppRoles.Administrator);

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
