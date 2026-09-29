using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;

namespace Munaqasat.Web.Pages.Admin;

public class IndexModel : PageModel
{
    public List<Tender> Tenders { get; set; } = MockData.Tenders;
    public List<TenderRequestRow> Requests { get; set; } = MockData.Requests;
    public List<TenderRequestRow> PendingRequests { get; set; } = MockData.Requests.Where(r => r.Status == "PENDING").ToList();
    public List<CompanyRow> Companies { get; set; } = MockData.CompanyRows;
    public List<UserRow> Users { get; set; } = MockData.UserRows;

    public void OnGet() { }
}
