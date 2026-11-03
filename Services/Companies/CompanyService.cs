using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Localization;
using CompanyView = Munaqasat.Web.Models.Company;
using CompanyRow = Munaqasat.Web.Models.CompanyRow;
using CustomSectionView = Munaqasat.Web.Models.CustomSection;
using ReviewView = Munaqasat.Web.Models.Review;

namespace Munaqasat.Web.Services.Companies;

public record CompanyInput(string Title, string? Website, string? Address, string? Bio, string? Sectors);

public interface ICompanyService
{
    Task<CompanyView?> GetProfileAsync(string id);
    Task<List<CompanyRow>> ListRowsAsync();
    Task<OperationResult> UpdateCurrentAsync(CompanyInput input);
}

public sealed class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IRequestLanguage _language;

    public CompanyService(ApplicationDbContext db, ICurrentUser current, IRequestLanguage language)
    {
        _db = db;
        _current = current;
        _language = language;
    }

    public async Task<CompanyView?> GetProfileAsync(string id)
    {
        var lang = _language.Code;

        var company = await _db.Companies
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Translations)
            .Include(c => c.CustomSections.OrderBy(s => s.SortOrder)).ThenInclude(s => s.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (company is null) return null;

        var ratings = await _db.CompanyRatings
            .AsNoTracking()
            .Where(r => r.RatedCompanyId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Quality, r.Communication, r.Experience, r.Deadline, r.Comment, r.Anonymous, r.CreatedAt,
                RaterTranslations = r.RatingCompany.Translations.ToList(),
                RaterDefault = r.RatingCompany.DefaultLanguage
            })
            .ToListAsync();

        var tr = Translations.Pick(company.Translations, lang, company.DefaultLanguage);
        var name = tr?.CompanyTitle ?? company.CompanyEmail;

        return new CompanyView
        {
            Id = company.Id,
            Name = name,
            Initials = Initials(name),
            Address = tr?.Address ?? "",
            Website = DisplayWebsite(company.CompanyWebsite),
            Sectors = company.Sectors ?? new List<string>(),
            Overall = Math.Round(company.AvgOverallRating, 1),
            RatingCount = company.NumberOfRatings,
            Bars = new Dictionary<string, double>
            {
                ["Quality"] = Math.Round(company.AvgQuality, 1),
                ["Communication"] = Math.Round(company.AvgCommunication, 1),
                ["Experience"] = Math.Round(company.AvgExperience, 1),
                ["Deadlines"] = Math.Round(company.AvgDeadline, 1)
            },
            Bio = tr?.Bio ?? "",
            CustomSections = company.CustomSections
                .Select(s => Translations.Pick(s.Translations, lang, company.DefaultLanguage))
                .Where(s => s is not null)
                .Select(s => new CustomSectionView { Tab = s!.TabName, Title = s.Title, Description = s.Description })
                .ToList(),
            Reviews = ratings.Select(r => new ReviewView
            {
                By = r.Anonymous ? "Anonymous" : Translations.Pick(r.RaterTranslations, lang, r.RaterDefault)?.CompanyTitle ?? "Company",
                Date = DateOnly.FromDateTime(r.CreatedAt.ToOffset(SystemClock.OmanOffset).DateTime),
                Quality = r.Quality,
                Communication = r.Communication,
                Experience = r.Experience,
                Deadline = r.Deadline,
                Comment = r.Comment ?? "",
                Anonymous = r.Anonymous
            }).ToList()
        };
    }

    public async Task<List<CompanyRow>> ListRowsAsync()
    {
        var lang = _language.Code;
        var companies = await _db.Companies
            .AsNoTracking()
            .Include(c => c.Translations)
            .OrderByDescending(c => c.AvgOverallRating)
            .ToListAsync();

        return companies.Select(c => new CompanyRow
        {
            Id = c.Id,
            Name = Translations.Pick(c.Translations, lang, c.DefaultLanguage)?.CompanyTitle ?? c.CompanyEmail,
            Sectors = string.Join(", ", (c.Sectors ?? new List<string>()).Take(2)),
            Rating = Math.Round(c.AvgOverallRating, 1),
            Count = c.NumberOfRatings,
            Status = c.VerificationStatus == VerificationStatuses.Verified ? "Verified" : "Pending review"
        }).ToList();
    }

    public async Task<OperationResult> UpdateCurrentAsync(CompanyInput input)
    {
        if (_current.CompanyId is null) return OperationResult.Failure("Your account is not linked to a company");
        if (string.IsNullOrWhiteSpace(input.Title)) return OperationResult.Failure("Company name is required");

        var lang = _language.Code;
        var company = await _db.Companies.Include(c => c.Translations).FirstOrDefaultAsync(c => c.Id == _current.CompanyId);
        if (company is null) return OperationResult.Failure("Company not found");

        company.CompanyWebsite = string.IsNullOrWhiteSpace(input.Website) ? null : input.Website.Trim();
        if (input.Sectors is not null)
        {
            company.Sectors = input.Sectors
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Edits are saved in the language the user is working in.
        var tr = company.Translations.FirstOrDefault(t => t.LanguageCode == lang);
        if (tr is null)
        {
            tr = new CompanyTranslation { LanguageCode = lang };
            company.Translations.Add(tr);
        }

        tr.CompanyTitle = input.Title.Trim();
        tr.Address = input.Address;
        tr.Bio = input.Bio;

        await _db.SaveChangesAsync();
        return OperationResult.Success();
    }

    public static string Initials(string name)
    {
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => char.IsLetter(w[0]))
            .ToList();
        return words.Count switch
        {
            0 => "?",
            1 => words[0][..Math.Min(2, words[0].Length)].ToUpperInvariant(),
            _ => string.Concat(words.Take(2).Select(w => char.ToUpperInvariant(w[0])))
        };
    }

    private static string DisplayWebsite(string? url) =>
        string.IsNullOrEmpty(url) ? "" : url.Replace("https://", "").Replace("http://", "").TrimEnd('/');
}
