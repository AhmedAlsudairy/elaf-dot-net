using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Tenders;

public sealed class TenderService : ITenderService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public TenderService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<TenderListItem>> SearchAsync(
        TenderQuery query,
        CancellationToken cancellationToken = default)
    {
        var today = _clock.Today;
        var tenders = _db.Tenders.AsNoTracking();

        if (query.Status is { } status)
            tenders = tenders.Where(t => t.Status == status);

        if (query.OnlyOpen)
            tenders = tenders.Where(t => t.ClosingOn >= today);

        if (query.Categories is { Count: > 0 })
            tenders = tenders.Where(t => query.Categories.Contains(t.Category));

        if (query.GovernorateId is { } governorateId)
            tenders = tenders.Where(t => t.GovernorateId == governorateId);

        if (query.BudgetMin is { } budgetMin)
            tenders = tenders.Where(t => t.BudgetMax == null || t.BudgetMax >= budgetMin);

        if (query.BudgetMax is { } budgetMax)
            tenders = tenders.Where(t => t.BudgetMin == null || t.BudgetMin <= budgetMax);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var pattern = $"%{query.SearchTerm.Trim()}%";
            tenders = tenders.Where(t =>
                EF.Functions.ILike(t.Title, pattern) ||
                EF.Functions.ILike(t.TitleAr, pattern) ||
                EF.Functions.ILike(t.Reference, pattern) ||
                EF.Functions.ILike(t.IssuingEntity, pattern));
        }

        var forCompanyId = query.ForCompanyId;

        if (query.MinMatchScore is { } minScore && forCompanyId is { } scoredCompanyId)
        {
            tenders = tenders.Where(t =>
                t.Matches.Any(m => m.CompanyId == scoredCompanyId && m.Score >= minScore));
        }

        var projected = tenders.Select(t => new TenderListItem
        {
            Id = t.Id,
            Reference = t.Reference,
            Title = t.Title,
            TitleAr = t.TitleAr,
            IssuingEntity = t.IssuingEntity,
            Category = t.Category,
            CategoryLabel = t.CategoryLabel,
            GovernorateName = t.Governorate == null ? null : t.Governorate.NameEn,
            WilayatName = t.Wilayat == null ? null : t.Wilayat.NameEn,
            BudgetMin = t.BudgetMin,
            BudgetMax = t.BudgetMax,
            Currency = t.Currency,
            PublishedOn = t.PublishedOn,
            ClosingOn = t.ClosingOn,
            Status = t.Status,
            Icon = t.Icon,
            BidCount = t.Bids.Count(b => b.Status != BidStatus.Withdrawn),
            MatchScore = forCompanyId == null
                ? null
                : t.Matches
                    .Where(m => m.CompanyId == forCompanyId)
                    .Select(m => (int?)m.Score)
                    .FirstOrDefault()
        });

        projected = query.Sort switch
        {
            TenderSort.ClosingSoonest => projected.OrderBy(t => t.ClosingOn),
            TenderSort.NewestFirst => projected.OrderByDescending(t => t.PublishedOn),
            TenderSort.BudgetHighest => projected.OrderByDescending(t => t.BudgetMax),

            _ => projected
                .OrderByDescending(t => t.MatchScore ?? -1)
                .ThenByDescending(t => t.PublishedOn)
        };

        return await projected.ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }

    public Task<Tender?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Tenders
            .AsNoTracking()
            .Include(t => t.Governorate)
            .Include(t => t.Wilayat)
            .Include(t => t.OwnerCompany)
            .Include(t => t.CustomFields.OrderBy(f => f.SortOrder))
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<OperationResult<Guid>> CreateAsync(
        TenderInput input,
        bool publishImmediately,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
            return OperationResult<Guid>.Failure("You must be signed in to create a tender.");

        if (Validate(input) is { } validationError)
            return OperationResult<Guid>.Failure(validationError);

        var tender = new Tender
        {
            OwnerCompanyId = _currentUser.CompanyId,
            CreatedByUserId = userId,
            Status = publishImmediately ? TenderStatus.Published : TenderStatus.Draft,
            PublishedOn = publishImmediately ? _clock.Today : null
        };

        Apply(input, tender);

        _db.Tenders.Add(tender);
        await _db.SaveChangesAsync(cancellationToken);

        return OperationResult<Guid>.Success(tender.Id);
    }

    public async Task<OperationResult> UpdateAsync(
        Guid id,
        TenderInput input,
        CancellationToken cancellationToken = default)
    {
        var tender = await _db.Tenders
            .Include(t => t.CustomFields)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tender is null) return OperationResult.Failure("Tender not found.");
        if (!CanManage(tender)) return OperationResult.Failure("You cannot edit this tender.");

        if (tender.Status is TenderStatus.Awarded or TenderStatus.Cancelled)
            return OperationResult.Failure("An awarded or cancelled tender can no longer be edited.");

        if (Validate(input) is { } validationError)
            return OperationResult.Failure(validationError);

        Apply(input, tender);

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task<OperationResult> PublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tender is null) return OperationResult.Failure("Tender not found.");
        if (!CanManage(tender)) return OperationResult.Failure("You cannot publish this tender.");
        if (tender.Status != TenderStatus.Draft) return OperationResult.Failure("Only a draft can be published.");
        if (tender.ClosingOn < _clock.Today) return OperationResult.Failure("Set a closing date in the future before publishing.");

        tender.Status = TenderStatus.Published;
        tender.PublishedOn = _clock.Today;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task<OperationResult> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tender is null) return OperationResult.Failure("Tender not found.");
        if (!CanManage(tender)) return OperationResult.Failure("You cannot close this tender.");
        if (tender.Status != TenderStatus.Published) return OperationResult.Failure("Only a published tender can be closed.");

        tender.Status = TenderStatus.Closed;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task<OperationResult> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tender is null) return OperationResult.Failure("Tender not found.");
        if (!CanManage(tender)) return OperationResult.Failure("You cannot cancel this tender.");
        if (tender.Status == TenderStatus.Awarded) return OperationResult.Failure("An awarded tender cannot be cancelled.");

        tender.Status = TenderStatus.Cancelled;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    private bool CanManage(Tender tender) =>
        _currentUser.IsAdministrator ||
        (tender.OwnerCompanyId is { } ownerId && ownerId == _currentUser.CompanyId);

    private string? Validate(TenderInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title)) return "A tender title is required.";
        if (string.IsNullOrWhiteSpace(input.Reference)) return "A tender number is required.";
        if (string.IsNullOrWhiteSpace(input.IssuingEntity)) return "The issuing entity is required.";
        if (input.ClosingOn < _clock.Today) return "The closing date cannot be in the past.";

        if (input.BudgetMin is { } min && input.BudgetMax is { } max && min > max)
            return "The minimum budget cannot be greater than the maximum.";

        if (input.BidBondPercentage is < 0 or > 100)
            return "The bid bond must be between 0 and 100 percent.";

        return null;
    }

    private static void Apply(TenderInput input, Tender tender)
    {
        tender.Reference = input.Reference.Trim();
        tender.Title = input.Title.Trim();
        tender.TitleAr = input.TitleAr.Trim();
        tender.Description = input.Description.Trim();
        tender.DescriptionAr = input.DescriptionAr.Trim();
        tender.IssuingEntity = input.IssuingEntity.Trim();
        tender.Category = input.Category;
        tender.CategoryLabel = input.CategoryLabel.Trim();
        tender.GovernorateId = input.GovernorateId;
        tender.WilayatId = input.WilayatId;
        tender.BudgetMin = input.BudgetMin;
        tender.BudgetMax = input.BudgetMax;
        tender.Currency = input.Currency;
        tender.DocumentFee = input.DocumentFee;
        tender.BidBondPercentage = input.BidBondPercentage;
        tender.ClosingOn = input.ClosingOn;
        tender.Icon = input.Icon;

        tender.CustomFields.Clear();

        var sortOrder = 0;
        foreach (var field in input.CustomFields.Where(f => !string.IsNullOrWhiteSpace(f.Title)))
        {
            tender.CustomFields.Add(new TenderCustomField
            {
                Title = field.Title.Trim(),
                Description = field.Description.Trim(),
                SortOrder = sortOrder++
            });
        }
    }
}
