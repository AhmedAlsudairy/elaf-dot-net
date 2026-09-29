using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;

namespace Munaqasat.Web.Pages.Tenders;

public class IndexModel : PageModel
{
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

    public void OnGet()
    {
        var query = MockData.Tenders.AsEnumerable();

        if (Category is { Count: > 0 })
            query = query.Where(t => Category.Contains(t.Category));

        if (!string.IsNullOrEmpty(Governorate))
            query = query.Where(t => t.Governorate == Governorate);

        if (MinScore > 0)
            query = query.Where(t => t.Score >= MinScore);

        if (!string.IsNullOrEmpty(Budget))
        {
            var parts = Budget.Split('-');
            var lo = decimal.Parse(parts[0]);
            var hi = decimal.Parse(parts[1]);
            query = query.Where(t => t.BudgetMax >= lo && t.BudgetMin <= hi);
        }

        query = Sort switch
        {
            "closing" => query.OrderBy(t => t.Closing),
            "newest" => query.OrderByDescending(t => t.Published),
            _ => query.OrderByDescending(t => t.Score)
        };

        Results = query.ToList();
    }
}
