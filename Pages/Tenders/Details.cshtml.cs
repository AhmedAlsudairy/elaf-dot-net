using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;

namespace Munaqasat.Web.Pages.Tenders;

public class DetailsModel : PageModel
{
    public Tender Tender { get; set; } = null!;
    public List<TenderRequestRow> Requests { get; set; } = new();

    public IActionResult OnGet(string id)
    {
        var tender = MockData.Tenders.FirstOrDefault(t => t.Id == id);
        if (tender is null) return NotFound();
        Tender = tender;
        Requests = MockData.Requests.Where(r => r.TenderId == id).ToList();
        return Page();
    }
}
