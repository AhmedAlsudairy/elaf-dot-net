using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Munaqasat.Web.Pages.Tenders;

public class CreateModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnPostDraft()
    {
        TempData["Toast"] = "Tender saved as draft";
        return RedirectToPage("Index");
    }

    public IActionResult OnPostPublish()
    {
        TempData["Toast"] = "Tender published";
        return RedirectToPage("Index");
    }
}
