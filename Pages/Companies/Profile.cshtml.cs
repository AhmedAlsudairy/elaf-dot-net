using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Companies;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Pages.Companies;

public class ProfileModel : PageModel
{
    private readonly ICompanyService _companies;
    private readonly ITenderService _tenders;

    public ProfileModel(ICompanyService companies, ITenderService tenders)
    {
        _companies = companies;
        _tenders = tenders;
    }

    public Company Company { get; set; } = null!;
    public List<Tender> SampleTenders { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var company = await _companies.GetProfileAsync(id);
        if (company is null) return NotFound();
        Company = company;
        SampleTenders = await _tenders.SearchAsync(new TenderFilter(Array.Empty<string>(), null, null, null, 0, "score", Take: 2));
        return Page();
    }
}
