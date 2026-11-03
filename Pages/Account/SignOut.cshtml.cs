using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Services.Accounts;

namespace Munaqasat.Web.Pages.Account;

public class SignOutModel : PageModel
{
    private readonly IAccountService _accounts;

    public SignOutModel(IAccountService accounts) => _accounts = accounts;

    public async Task<IActionResult> OnGetAsync()
    {
        await _accounts.SignOutAsync();
        Response.Cookies.Delete("munaqasat_role");
        return Redirect("/");
    }
}
