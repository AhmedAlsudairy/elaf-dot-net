using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Models;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Localization;
using TenderEntity = Munaqasat.Web.Domain.Entities.Tender;
using TenderView = Munaqasat.Web.Models.Tender;
using CustomFieldView = Munaqasat.Web.Models.CustomField;

namespace Munaqasat.Web.Services.Tenders;

public record TenderFilter(
    IReadOnlyCollection<string> Categories,
    string? Governorate,
    decimal? BudgetLow,
    decimal? BudgetHigh,
    int MinScore,
    string Sort,
    int? Take = null,
    bool IncludeDrafts = false);

public class CreateTenderInput
{
    public string Entity { get; set; } = "";
    public string Title { get; set; } = "";
    public string TenderNo { get; set; } = "";
    public string ExtId { get; set; } = "";
    public string Category { get; set; } = "";
    public string TenderType { get; set; } = "";
    public string Governorate { get; set; } = "";
    public string Wilayat { get; set; } = "";
    public bool AlternateBids { get; set; }
    public DateOnly? Published { get; set; }
    public DateTime? SaleStart { get; set; }
    public DateTime? SaleEnd { get; set; }
    public DateTime? ClarStart { get; set; }
    public DateTime? ClarEnd { get; set; }
    public DateTime? Closing { get; set; }
    public DateTime? Opening { get; set; }
    public decimal? DocFee { get; set; }
    public string FeeMode { get; set; } = "";
    public decimal? Bond { get; set; }
    public bool EBond { get; set; }
    public string Description { get; set; } = "";
    public List<CustomFieldView> CustomFields { get; set; } = new();
}

public interface ITenderService
{
    Task<List<TenderView>> SearchAsync(TenderFilter filter);
    Task<TenderView?> GetAsync(string id);
    Task<List<TenderRequestRow>> GetRequestsAsync(string? tenderId = null);
    Task<OperationResult<string>> CreateAsync(CreateTenderInput input, bool publish);
    Task<OperationResult> SubmitRequestAsync(string tenderId, decimal? price, int? days, string? description);
    Task<OperationResult> SetRequestStatusAsync(string requestId, RequestStatus status);
}

public sealed class TenderService : ITenderService
{
    private static readonly TimeSpan NewWindow = TimeSpan.FromDays(7);

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IRequestLanguage _language;
    private readonly IClock _clock;

    public TenderService(ApplicationDbContext db, ICurrentUser current, IRequestLanguage language, IClock clock)
    {
        _db = db;
        _current = current;
        _language = language;
        _clock = clock;
    }

    public async Task<List<TenderView>> SearchAsync(TenderFilter filter)
    {
        var query = BaseQuery();

        if (!filter.IncludeDrafts) query = query.Where(t => t.Status == TenderStatuses.Published);

        if (filter.Categories.Count > 0)
            query = query.Where(t => filter.Categories.Contains(t.ProcurementCategoryCode));

        if (!string.IsNullOrEmpty(filter.Governorate))
            query = query.Where(t => t.Translations.Any(tr => tr.Governorate == filter.Governorate));

        if (filter.BudgetLow is { } lo && filter.BudgetHigh is { } hi)
            query = query.Where(t => t.BudgetMax >= lo && t.BudgetMin <= hi);

        var tenders = await query.ToListAsync();
        var matcher = await LoadMatcherAsync();

        IEnumerable<TenderView> views = tenders.Select(t => ToView(t, matcher));

        if (filter.MinScore > 0) views = views.Where(v => v.Score >= filter.MinScore);

        views = filter.Sort switch
        {
            "closing" => views.OrderBy(v => v.Closing),
            "newest" => views.OrderByDescending(v => v.Published),
            _ => views.OrderByDescending(v => v.Score).ThenBy(v => v.Closing)
        };

        if (filter.Take is { } take) views = views.Take(take);

        return views.ToList();
    }

    public async Task<TenderView?> GetAsync(string id)
    {
        var tender = await BaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tender is null) return null;
        if (tender.Status != TenderStatuses.Published && !_current.IsAdministrator) return null;

        return ToView(tender, await LoadMatcherAsync());
    }

    public async Task<List<TenderRequestRow>> GetRequestsAsync(string? tenderId = null)
    {
        var lang = _language.Code;
        var query = _db.TenderRequests.AsNoTracking();
        if (tenderId is not null) query = query.Where(r => r.TenderId == tenderId);

        var rows = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.TenderId, r.Price, r.Days, r.Status, r.CreatedAt,
                CompanyTranslations = r.Company.Translations.ToList(),
                CompanyDefault = r.Company.DefaultLanguage,
                r.Company.CompanyEmail
            })
            .ToListAsync();

        return rows.Select(r => new TenderRequestRow
        {
            Id = r.Id,
            TenderId = r.TenderId,
            Company = Translations.Pick(r.CompanyTranslations, lang, r.CompanyDefault)?.CompanyTitle ?? r.CompanyEmail,
            Price = r.Price ?? 0,
            Days = r.Days ?? 0,
            Status = r.Status.ToString().ToUpperInvariant(),
            Date = ToOmanDate(r.CreatedAt)
        }).ToList();
    }

    public async Task<OperationResult<string>> CreateAsync(CreateTenderInput input, bool publish)
    {
        if (string.IsNullOrWhiteSpace(input.Entity)) return OperationResult<string>.Failure("Issuing entity is required");
        if (string.IsNullOrWhiteSpace(input.Title)) return OperationResult<string>.Failure("Tender title is required");
        if (string.IsNullOrWhiteSpace(input.TenderNo)) return OperationResult<string>.Failure("Tender no. is required");
        if (input.Closing is null) return OperationResult<string>.Failure("Bid submission closing is required");
        if (input.Bond is < 0 or > 100) return OperationResult<string>.Failure("Bid bond must be between 0 and 100%");
        if (input.DocFee is < 0) return OperationResult<string>.Failure("Document fee cannot be negative");

        var category = input.Category.Trim().ToUpperInvariant();
        if (!ProcurementCategories.All.Contains(category))
            return OperationResult<string>.Failure("Choose a procurement category");

        var externalId = string.IsNullOrWhiteSpace(input.ExtId) ? $"LOCAL-{Ids.New()[..8].ToUpperInvariant()}" : input.ExtId.Trim();
        if (await _db.Tenders.AnyAsync(t => t.ExternalTenderId == externalId))
            return OperationResult<string>.Failure($"A tender with external ID {externalId} already exists");

        var lang = _language.Code;
        var tender = new TenderEntity
        {
            ExternalTenderId = externalId,
            TenderNo = input.TenderNo.Trim(),
            ProcurementCategoryCode = category,
            TenderTypeCode = ToCode(input.TenderType, "OPEN_LOCAL"),
            IsAlternateBidAllowed = input.AlternateBids,
            DocumentFee = input.DocFee,
            DocumentFeePaymentMode = string.IsNullOrWhiteSpace(input.FeeMode) ? null : ToCode(input.FeeMode, "ONLINE_PAYMENT"),
            BondPercentage = input.Bond,
            IsEBondApplicable = input.EBond,
            OriginalLanguage = lang,
            PublishedDate = input.Published ?? _clock.Today,
            DocumentSalesStart = FromOmanLocal(input.SaleStart),
            DocumentSalesEnd = FromOmanLocal(input.SaleEnd),
            PrebidClarificationStart = FromOmanLocal(input.ClarStart),
            PrebidClarificationEnd = FromOmanLocal(input.ClarEnd),
            BidSubmissionClosing = FromOmanLocal(input.Closing)!.Value,
            PricedBidOpening = FromOmanLocal(input.Opening),
            Status = publish ? TenderStatuses.Published : TenderStatuses.Draft
        };

        tender.Translations.Add(new TenderTranslation
        {
            LanguageCode = lang,
            EntityName = input.Entity.Trim(),
            Title = input.Title.Trim(),
            ProcurementCategory = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(category.ToLowerInvariant()),
            Governorate = NullIfBlank(input.Governorate),
            Wilayat = NullIfBlank(input.Wilayat),
            Note = NullIfBlank(input.Description)
        });

        var order = 0;
        foreach (var field in input.CustomFields.Where(f => !string.IsNullOrWhiteSpace(f.Title)))
        {
            var cf = new Munaqasat.Web.Domain.Entities.CustomField { SortOrder = order++ };
            cf.Translations.Add(new CustomFieldTranslation
            {
                LanguageCode = lang,
                Title = field.Title.Trim(),
                Description = field.Description?.Trim() ?? ""
            });
            tender.CustomFields.Add(cf);
        }

        _db.Tenders.Add(tender);
        await _db.SaveChangesAsync();
        return OperationResult<string>.Success(tender.Id);
    }

    public async Task<OperationResult> SubmitRequestAsync(string tenderId, decimal? price, int? days, string? description)
    {
        if (_current.CompanyId is not { } companyId)
            return OperationResult.Failure("Only company accounts can submit requests");
        if (price is < 0) return OperationResult.Failure("Price cannot be negative");
        if (days is <= 0) return OperationResult.Failure("Delivery days must be more than zero");

        var tender = await _db.Tenders.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenderId);
        if (tender is null || tender.Status != TenderStatuses.Published) return OperationResult.Failure("Tender not found");
        if (tender.BidSubmissionClosing < _clock.UtcNow) return OperationResult.Failure("Bid submission for this tender has closed");

        if (await _db.TenderRequests.AnyAsync(r => r.TenderId == tenderId && r.CompanyProfileId == companyId))
            return OperationResult.Failure("Your company has already submitted a request for this tender");

        _db.TenderRequests.Add(new TenderRequest
        {
            TenderId = tenderId,
            CompanyProfileId = companyId,
            Price = price,
            Days = days,
            Description = NullIfBlank(description),
            LanguageCode = _language.Code
        });
        await _db.SaveChangesAsync();
        return OperationResult.Success();
    }

    public async Task<OperationResult> SetRequestStatusAsync(string requestId, RequestStatus status)
    {
        var request = await _db.TenderRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request is null) return OperationResult.Failure("Request not found");

        request.Status = status;
        await _db.SaveChangesAsync();
        return OperationResult.Success();
    }

    private IQueryable<TenderEntity> BaseQuery() => _db.Tenders
        .AsNoTracking()
        .AsSplitQuery()
        .Include(t => t.Translations)
        .Include(t => t.CustomFields.OrderBy(f => f.SortOrder)).ThenInclude(f => f.Translations);

    private TenderView ToView(TenderEntity t, TenderMatcher matcher)
    {
        var lang = _language.Code;
        var tr = Translations.Pick(t.Translations, lang, t.OriginalLanguage);
        var categoryLabel = tr?.ProcurementSubCategory ?? tr?.ProcurementCategory
            ?? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(t.ProcurementCategoryCode.ToLowerInvariant());

        var view = new TenderView
        {
            Id = t.Id,
            Entity = tr?.EntityName ?? "",
            Title = tr?.Title ?? t.TenderNo,
            TenderNo = t.TenderNo,
            ExtId = t.ExternalTenderId,
            Category = t.ProcurementCategoryCode,
            CategoryLabel = categoryLabel,
            Governorate = tr?.Governorate ?? "",
            Wilayat = tr?.Wilayat ?? "",
            BudgetMin = t.BudgetMin ?? 0,
            BudgetMax = t.BudgetMax ?? 0,
            BudgetLabel = BudgetLabel(t.BudgetMin, t.BudgetMax),
            Published = t.PublishedDate,
            Closing = ToOmanDate(t.BidSubmissionClosing),
            DocFee = t.DocumentFee is { } fee ? $"OMR {fee.ToString("0.000", CultureInfo.InvariantCulture)}" : "None",
            Bond = t.BondPercentage is { } bond ? $"{bond.ToString("0.0", CultureInfo.InvariantCulture)}% of quoted value" : "Not required",
            Status = t.Status != TenderStatuses.Published ? "Draft"
                : t.BidSubmissionClosing < _clock.UtcNow ? "Closed" : "Open",
            IsNew = _clock.Today.DayNumber - t.PublishedDate.DayNumber <= NewWindow.Days,
            Icon = t.ProcurementCategoryCode switch
            {
                ProcurementCategories.Supply => "i-box",
                ProcurementCategories.Works => "i-tool",
                _ => "i-briefcase"
            },
            Description = tr?.Note ?? "",
            CustomFields = t.CustomFields
                .Select(f => Translations.Pick(f.Translations, lang, t.OriginalLanguage))
                .Where(f => f is not null)
                .Select(f => new CustomFieldView { Title = f!.Title, Description = f.Description })
                .ToList()
        };

        view.Score = matcher.Score(t);
        return view;
    }

    private async Task<TenderMatcher> LoadMatcherAsync()
    {
        if (_current.CompanyId is not { } companyId) return TenderMatcher.Empty;

        var company = await _db.Companies.AsNoTracking()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == companyId);
        if (company is null) return TenderMatcher.Empty;

        var addresses = company.Translations.Select(t => t.Address).Where(a => !string.IsNullOrEmpty(a)).ToList();
        return new TenderMatcher(company.Sectors ?? new List<string>(), addresses!);
    }

    private static string BudgetLabel(decimal? min, decimal? max)
    {
        if (min is null && max is null) return "Budget not disclosed";
        if (min is { } lo && max is { } hi) return $"OMR {Short(lo)}–{Short(hi)}";
        return $"OMR {Short((min ?? max)!.Value)}";

        static string Short(decimal v) => v >= 1_000_000
            ? (v / 1_000_000m).ToString("0.#", CultureInfo.InvariantCulture) + "M"
            : v >= 1_000 ? (v / 1_000m).ToString("0.#", CultureInfo.InvariantCulture) + "K"
            : v.ToString("0", CultureInfo.InvariantCulture);
    }

    private static DateOnly ToOmanDate(DateTimeOffset value) =>
        DateOnly.FromDateTime(value.ToOffset(SystemClock.OmanOffset).DateTime);

    /// <summary>The form's datetime-local inputs are Muscat wall-clock time.</summary>
    private static DateTimeOffset? FromOmanLocal(DateTime? local) =>
        local is { } value
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), SystemClock.OmanOffset).ToUniversalTime()
            : null;

    private static string ToCode(string label, string fallback)
    {
        var code = Regex.Replace(label.ToUpperInvariant(), "[^A-Z0-9]+", "_").Trim('_');
        return code.Length == 0 ? fallback : code;
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>
/// Scores a tender 0-100 for the signed-in user's company: sector overlap with the tender's
/// category label or title, plus a bonus when the company's address names the tender's governorate.
/// </summary>
public sealed class TenderMatcher
{
    private const int BaseScore = 20;
    private const int CategoryMatchPoints = 55;
    private const int TitleMatchPoints = 30;
    private const int SameGovernoratePoints = 25;

    public static readonly TenderMatcher Empty = new(new List<string>(), new List<string>());

    private readonly IReadOnlyList<string> _sectors;
    private readonly IReadOnlyList<string> _addresses;

    public TenderMatcher(IReadOnlyList<string> sectors, IReadOnlyList<string> addresses)
    {
        _sectors = sectors;
        _addresses = addresses;
    }

    /// <summary>Checks every translation, so the score does not change with the display language.</summary>
    public int Score(TenderEntity tender)
    {
        var score = BaseScore;
        var translations = tender.Translations.ToList();

        if (_sectors.Any(s => translations.Any(t => Contains(t.ProcurementSubCategory, s) || Contains(t.ProcurementCategory, s))))
            score += CategoryMatchPoints;
        else if (_sectors.Any(s => translations.Any(t => Contains(t.Title, s) || Contains(t.Note, s))))
            score += TitleMatchPoints;

        if (_addresses.Any(a => translations.Any(t => Contains(a, t.Governorate))))
            score += SameGovernoratePoints;

        return Math.Clamp(score, 0, 100);
    }

    private static bool Contains(string? haystack, string? needle) =>
        !string.IsNullOrEmpty(haystack) && !string.IsNullOrEmpty(needle)
        && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
