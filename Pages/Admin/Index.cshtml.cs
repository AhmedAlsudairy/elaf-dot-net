using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Accounts;
using Munaqasat.Web.Services.Companies;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ITenderService _tenders;
    private readonly ICompanyService _companies;
    private readonly IAccountService _accounts;

    public IndexModel(ITenderService tenders, ICompanyService companies, IAccountService accounts)
    {
        _tenders = tenders;
        _companies = companies;
        _accounts = accounts;
    }

    public List<Tender> Tenders { get; set; } = new();
    public List<TenderRequestRow> Requests { get; set; } = new();
    public List<TenderRequestRow> PendingRequests { get; set; } = new();
    public List<CompanyRow> Companies { get; set; } = new();
    public List<UserRow> Users { get; set; } = new();
    public int OpenTenderCount => Tenders.Count(t => t.Status == "Open");

    public async Task OnGetAsync()
    {
        Tenders = await _tenders.SearchAsync(new TenderFilter(Array.Empty<string>(), null, null, null, 0, "newest", IncludeDrafts: true));
        Requests = await _tenders.GetRequestsAsync();
        PendingRequests = Requests.Where(r => r.Status == "PENDING").ToList();
        Companies = await _companies.ListRowsAsync();
        Users = await _accounts.ListUsersAsync();
    }

    /// <summary>Called by adminSetStatus() in site.js.</summary>
    public async Task<IActionResult> OnPostRequestStatusAsync(string requestId, string status)
    {
        if (!Enum.TryParse<RequestStatus>(status, ignoreCase: true, out var parsed)) return BadRequest();
        var result = await _tenders.SetRequestStatusAsync(requestId, parsed);
        return result.Succeeded ? new JsonResult(new { ok = true }) : NotFound(new { error = result.Error });
    }
}
