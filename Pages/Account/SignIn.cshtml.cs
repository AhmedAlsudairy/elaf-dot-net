using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Munaqasat.Web.Pages.Account;

public class SignInModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnPost(string email, string password)
    {
        // UI-only prototype: no real authentication, just move to the app shell.
        return RedirectToPage("/Tenders/Index");
    }
}
