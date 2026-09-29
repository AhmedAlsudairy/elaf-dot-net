using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;

namespace Munaqasat.Web.Pages;

public class IndexModel : PageModel
{
    public List<Tender> HeroTenders { get; set; } = new();
    public List<Tier> Tiers { get; set; } = MockData.Tiers;

    public void OnGet()
    {
        HeroTenders = MockData.Tenders.Take(3).ToList();
    }

    public IActionResult OnPostContact(string name, string email, string message)
    {
        TempData["Toast"] = "Message sent — we will reply within one business day";
        return RedirectToPage("Index", pageHandler: null, fragment: "contact");
    }
}
