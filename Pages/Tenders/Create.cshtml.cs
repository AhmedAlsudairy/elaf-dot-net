using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Pages.Tenders;

public class CreateModel : PageModel
{
    private readonly ITenderService _tenders;

    public CreateModel(ITenderService tenders) => _tenders = tenders;

    public void OnGet() { }

    // The form posts flat names (Entity, Title, CustomFields[0].Title, ...), which bind to the parameter directly.
    public Task<IActionResult> OnPostDraftAsync(CreateTenderInput input) => SaveAsync(input, publish: false);

    public Task<IActionResult> OnPostPublishAsync(CreateTenderInput input) => SaveAsync(input, publish: true);

    private async Task<IActionResult> SaveAsync(CreateTenderInput input, bool publish)
    {
        var result = await _tenders.CreateAsync(input, publish);
        if (!result.Succeeded)
        {
            TempData["Toast"] = result.Error;
            return Page();
        }

        TempData["Toast"] = publish ? "Tender published" : "Tender saved as draft";
        return publish ? RedirectToPage("Details", new { id = result.Value }) : RedirectToPage("Index");
    }
}
