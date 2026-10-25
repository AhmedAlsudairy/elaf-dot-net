using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Companies;

public sealed class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public CompanyService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        ProfileQuery().FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        ProfileQuery().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<OperationResult<Guid>> CreateAsync(
        CompanyProfileInput input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return OperationResult<Guid>.Failure("A company name is required.");

        if (!string.IsNullOrWhiteSpace(input.CommercialRegistrationNumber))
        {
            var registration = input.CommercialRegistrationNumber.Trim();
            var taken = await _db.Companies
                .AnyAsync(c => c.CommercialRegistrationNumber == registration, cancellationToken);

            if (taken)
                return OperationResult<Guid>.Failure("A company with that commercial registration number already exists.");
        }

        var company = new Company
        {
            Slug = await ReserveSlugAsync(input.Name, excludingCompanyId: null, cancellationToken),
            Initials = InitialsFrom(input.Name),
            VerificationStatus = CompanyVerificationStatus.Pending
        };

        Apply(input, company);

        _db.Companies.Add(company);
        await ReplaceSectorsAsync(company, input.SectorIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return OperationResult<Guid>.Success(company.Id);
    }

    public async Task<OperationResult> UpdateProfileAsync(
        Guid id,
        CompanyProfileInput input,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdministrator && _currentUser.CompanyId != id)
            return OperationResult.Failure("You cannot edit this company profile.");

        var company = await _db.Companies
            .Include(c => c.Sectors)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (company is null) return OperationResult.Failure("Company not found.");

        if (string.IsNullOrWhiteSpace(input.Name))
            return OperationResult.Failure("A company name is required.");

        if (!string.IsNullOrWhiteSpace(input.CommercialRegistrationNumber))
        {
            var registration = input.CommercialRegistrationNumber.Trim();
            var taken = await _db.Companies
                .AnyAsync(c => c.Id != id && c.CommercialRegistrationNumber == registration, cancellationToken);

            if (taken)
                return OperationResult.Failure("A company with that commercial registration number already exists.");
        }

        if (!string.Equals(company.Name, input.Name.Trim(), StringComparison.Ordinal))
        {
            var candidate = Slug.From(input.Name);
            if (candidate.Length > 0 && !company.Slug.StartsWith(candidate, StringComparison.Ordinal))
                company.Slug = await ReserveSlugAsync(input.Name, id, cancellationToken);
        }

        Apply(input, company);
        await ReplaceSectorsAsync(company, input.SectorIds, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task<OperationResult> SetVerificationStatusAsync(
        Guid id,
        CompanyVerificationStatus status,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdministrator)
            return OperationResult.Failure("Only an administrator can change verification status.");

        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (company is null) return OperationResult.Failure("Company not found.");

        company.VerificationStatus = status;
        company.VerifiedAt = status == CompanyVerificationStatus.Verified ? _clock.UtcNow : null;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task RecalculateRatingAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company is null) return;

        var rollup = await _db.Reviews
            .Where(r => r.CompanyId == companyId && r.IsPublished)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r =>
                    (r.QualityRating + r.CommunicationRating + r.ExperienceRating + r.DeadlineRating) / 4d)
            })
            .FirstOrDefaultAsync(cancellationToken);

        company.RatingCount = rollup?.Count ?? 0;
        company.RatingAverage = rollup is null ? 0 : Math.Round(rollup.Average, 2);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Company> ProfileQuery() =>
        _db.Companies
            .AsNoTracking()
            .Include(c => c.Governorate)
            .Include(c => c.Sectors).ThenInclude(cs => cs.Sector)
            .Include(c => c.ProfileSections.OrderBy(s => s.SortOrder))
            .Include(c => c.Reviews.Where(r => r.IsPublished).OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.AuthorUser);

    private static void Apply(CompanyProfileInput input, Company company)
    {
        company.Name = input.Name.Trim();
        company.NameAr = input.NameAr.Trim();
        company.CommercialRegistrationNumber = string.IsNullOrWhiteSpace(input.CommercialRegistrationNumber)
            ? null
            : input.CommercialRegistrationNumber.Trim();
        company.Address = input.Address.Trim();
        company.Website = string.IsNullOrWhiteSpace(input.Website) ? null : input.Website.Trim();
        company.Bio = input.Bio.Trim();
        company.BioAr = input.BioAr.Trim();
        company.GovernorateId = input.GovernorateId;
    }

    private async Task ReplaceSectorsAsync(
        Company company,
        IReadOnlyCollection<Guid> sectorIds,
        CancellationToken cancellationToken)
    {
        var wanted = sectorIds.Distinct().ToHashSet();

        var valid = await _db.Sectors
            .Where(s => wanted.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        company.Sectors.Clear();

        foreach (var sectorId in valid)
            company.Sectors.Add(new CompanySector { CompanyId = company.Id, SectorId = sectorId });
    }

    private async Task<string> ReserveSlugAsync(
        string name,
        Guid? excludingCompanyId,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slug.From(name);
        if (baseSlug.Length == 0) baseSlug = "company";

        var taken = await _db.Companies
            .Where(c => c.Slug.StartsWith(baseSlug) && c.Id != excludingCompanyId)
            .Select(c => c.Slug)
            .ToListAsync(cancellationToken);

        if (!taken.Contains(baseSlug)) return baseSlug;

        for (var suffix = 2; suffix < 1000; suffix++)
        {
            var candidate = $"{baseSlug}-{suffix}";
            if (!taken.Contains(candidate)) return candidate;
        }

        return $"{baseSlug}-{Guid.NewGuid():N}"[..40];
    }

    private static string InitialsFrom(string name)
    {
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return words.Length switch
        {
            0 => "",
            1 => words[0][..Math.Min(2, words[0].Length)].ToUpperInvariant(),
            _ => $"{words[0][0]}{words[1][0]}".ToUpperInvariant()
        };
    }
}
