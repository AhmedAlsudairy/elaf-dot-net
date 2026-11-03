using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Munaqasat.Web.Data;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Accounts;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Companies;
using Munaqasat.Web.Services.Localization;

namespace Munaqasat.Web.Pages.Settings;

public class IndexModel : PageModel
{
    private readonly IAccountService _accounts;
    private readonly ICompanyService _companies;

    public IndexModel(IAccountService accounts, ICompanyService companies)
    {
        _accounts = accounts;
        _companies = companies;
    }

    public List<Tier> Tiers { get; set; } = Pricing.Tiers;
    public SettingsView? View { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        View = await _accounts.GetSettingsAsync();
        return View is null ? RedirectToPage("/Account/SignIn") : Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(
        string section,
        string? name, string? email, string? phone, string? timezone, string? bio,
        string? companyTitle, string? website, string? address, string? companyBio, string? sectors,
        string? language,
        string? newPassword, string? confirmPassword)
    {
        OperationResult result;
        string success;

        switch (section)
        {
            case "profile":
                result = await _accounts.UpdateProfileAsync(new ProfileInput(name ?? "", email ?? "", phone, timezone ?? "", bio));
                success = "Profile updated";
                break;
            case "company":
                result = await _companies.UpdateCurrentAsync(new CompanyInput(companyTitle ?? "", website, address, companyBio, sectors));
                success = "Company profile updated";
                break;
            case "language":
                result = await _accounts.SetLanguageAsync(language ?? Translations.Fallback, timezone ?? "");
                if (result.Succeeded)
                {
                    Response.Cookies.Append(RequestLanguage.CookieName, language ?? Translations.Fallback,
                        new CookieOptions { MaxAge = TimeSpan.FromDays(365), IsEssential = true });
                }
                success = "Language preference saved";
                break;
            case "security":
                result = await _accounts.ChangePasswordAsync(newPassword ?? "", confirmPassword ?? "");
                success = "Security settings updated";
                break;
            default:
                return BadRequest();
        }

        TempData["Toast"] = result.Succeeded ? success : result.Error;
        return RedirectToPage();
    }
}
