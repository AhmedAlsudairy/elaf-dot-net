using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;

namespace Munaqasat.Web.Pages.Companies;

public class ProfileModel : PageModel
{
    public Company Company { get; set; } = null!;
    public List<Tender> SampleTenders { get; set; } = new();

    public IActionResult OnGet(string id)
    {
        if (!MockData.Companies.TryGetValue(id, out var company)) return NotFound();
        Company = company;
        SampleTenders = MockData.Tenders.Take(2).ToList();
        return Page();
    }
}
