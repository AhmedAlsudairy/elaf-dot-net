using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Services.Accounts;

namespace Munaqasat.Web.Pages.Account;

public class SignInModel : PageModel
{
    private readonly IAccountService _accounts;

    public SignInModel(IAccountService accounts) => _accounts = accounts;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string email, string password, string? returnUrl)
    {
        var result = await _accounts.SignInAsync(email ?? "", password ?? "");
        if (!result.Succeeded)
        {
            TempData["Toast"] = result.Error;
            return Page();
        }

        // The role cookie drives the app shell's navigation; reset it so it reflects this account.
        Response.Cookies.Delete("munaqasat_role");
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToPage("/Tenders/Index");
    }
}
