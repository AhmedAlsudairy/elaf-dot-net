using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Services.Accounts;

namespace Munaqasat.Web.Pages.Account;

public class SignUpModel : PageModel
{
    private readonly IAccountService _accounts;

    public SignUpModel(IAccountService accounts) => _accounts = accounts;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string accountType, string fullName, string orgName, string email, string phone, string password)
    {
        var result = await _accounts.SignUpAsync(new SignUpInput(accountType ?? "", fullName ?? "", orgName, email ?? "", phone, password ?? ""));
        if (!result.Succeeded)
        {
            TempData["Toast"] = result.Error;
            return Page();
        }

        Response.Cookies.Delete("munaqasat_role");
        return RedirectToPage("/Tenders/Index");
    }
}
