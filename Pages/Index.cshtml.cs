using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ITenderService _tenders;

    public IndexModel(ITenderService tenders) => _tenders = tenders;

    public List<Tender> HeroTenders { get; set; } = new();
    public List<Tier> Tiers { get; set; } = Pricing.Tiers;

    public async Task OnGetAsync()
    {
        HeroTenders = await _tenders.SearchAsync(new TenderFilter(Array.Empty<string>(), null, null, null, 0, "newest", Take: 3));
    }

    public IActionResult OnPostContact(string name, string email, string message)
    {
        TempData["Toast"] = "Message sent — we will reply within one business day";
        return RedirectToPage("Index", pageHandler: null, fragment: "contact");
    }
}
