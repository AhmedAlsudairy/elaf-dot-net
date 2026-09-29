using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;

namespace Munaqasat.Web.Pages.Settings;

public class IndexModel : PageModel
{
    public List<Tier> Tiers { get; set; } = MockData.Tiers;

    public void OnGet() { }

    public IActionResult OnPostSave(string section)
    {
        TempData["Toast"] = section switch
        {
            "profile" => "Profile updated",
            "company" => "Company profile updated",
            "language" => "Language preference saved",
            "security" => "Security settings updated",
            _ => "Settings saved"
        };
        return RedirectToPage();
    }
}
