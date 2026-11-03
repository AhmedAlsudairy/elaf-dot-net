using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Pages.Tenders;

public class DetailsModel : PageModel
{
    private readonly ITenderService _tenders;
    private readonly ICurrentUser _current;

    public DetailsModel(ITenderService tenders, ICurrentUser current)
    {
        _tenders = tenders;
        _current = current;
    }

    public Tender Tender { get; set; } = null!;
    public List<TenderRequestRow> Requests { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var tender = await _tenders.GetAsync(id);
        if (tender is null) return NotFound();
        Tender = tender;
        Requests = await _tenders.GetRequestsAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostRequestAsync(string id, decimal? price, int? days, string? description)
    {
        if (!_current.IsAuthenticated) return Challenge();

        var result = await _tenders.SubmitRequestAsync(id, price, days, description);
        TempData["Toast"] = result.Succeeded ? "Request submitted" : result.Error;
        return RedirectToPage(new { id });
    }
}
