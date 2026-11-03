using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Pages.Tenders;

public class IndexModel : PageModel
{
    private readonly ITenderService _tenders;

    public IndexModel(ITenderService tenders) => _tenders = tenders;

    [BindProperty(SupportsGet = true)]
    public List<string> Category { get; set; } = new() { "SUPPLY", "SERVICES", "WORKS" };

    [BindProperty(SupportsGet = true)]
    public string Governorate { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Budget { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public int MinScore { get; set; } = 0;

    [BindProperty(SupportsGet = true)]
    public string Sort { get; set; } = "score";

    public List<Tender> Results { get; set; } = new();

    public async Task OnGetAsync()
    {
        decimal? lo = null, hi = null;
        var parts = Budget.Split('-');
        if (parts.Length == 2
            && decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var l)
            && decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var h))
        {
            lo = l;
            hi = h;
        }

        Results = await _tenders.SearchAsync(new TenderFilter(
            Category, string.IsNullOrEmpty(Governorate) ? null : Governorate, lo, hi, MinScore, Sort));
    }
}
