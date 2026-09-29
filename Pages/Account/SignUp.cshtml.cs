using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Munaqasat.Web.Pages.Account;

public class SignUpModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnPost(string accountType, string fullName, string orgName, string email, string phone, string password)
    {
        // UI-only prototype: no real account creation, just carry the chosen role forward.
        Response.Cookies.Append("munaqasat_role", string.IsNullOrEmpty(accountType) ? "client" : accountType,
            new CookieOptions { MaxAge = TimeSpan.FromDays(30) });
        return RedirectToPage("/Tenders/Index");
    }
}
