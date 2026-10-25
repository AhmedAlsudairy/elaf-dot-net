using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Matching;

public interface ITenderMatchService
{
    Task<int> RecomputeForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<int> RecomputeForTenderAsync(Guid tenderId, CancellationToken cancellationToken = default);
}

public sealed class TenderMatchService : ITenderMatchService
{
    private readonly ApplicationDbContext _db;
    private readonly ITenderMatchScorer _scorer;
    private readonly IClock _clock;

    public TenderMatchService(ApplicationDbContext db, ITenderMatchScorer scorer, IClock clock)
    {
        _db = db;
        _scorer = scorer;
        _clock = clock;
    }

    public async Task<int> RecomputeForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var profile = await LoadProfileAsync(companyId, cancellationToken);
        if (profile is null) return 0;

        var candidates = await OpenTenders().ToListAsync(cancellationToken);

        var existing = await _db.TenderMatches
            .Where(m => m.CompanyId == companyId)
            .ToDictionaryAsync(m => m.TenderId, cancellationToken);

        foreach (var candidate in candidates)
            Upsert(existing, candidate, profile);

        await _db.SaveChangesAsync(cancellationToken);
        return candidates.Count;
    }

    public async Task<int> RecomputeForTenderAsync(Guid tenderId, CancellationToken cancellationToken = default)
    {
        var candidate = await OpenTenders()
            .FirstOrDefaultAsync(t => t.TenderId == tenderId, cancellationToken);

        if (candidate is null) return 0;

        var companyIds = await _db.Companies
            .Where(c => c.VerificationStatus == CompanyVerificationStatus.Verified)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var existing = await _db.TenderMatches
            .Where(m => m.TenderId == tenderId)
            .ToDictionaryAsync(m => m.CompanyId, cancellationToken);

        foreach (var companyId in companyIds)
        {
            var profile = await LoadProfileAsync(companyId, cancellationToken);
            if (profile is null) continue;

            var score = _scorer.Score(candidate, profile);

            if (existing.TryGetValue(companyId, out var match))
            {
                match.Score = score.Value;
                match.Reason = score.Reason;
                match.ComputedAt = _clock.UtcNow;
            }
            else
            {
                _db.TenderMatches.Add(NewMatch(candidate.TenderId, companyId, score, _clock.UtcNow));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return companyIds.Count;
    }

    private void Upsert(
        IReadOnlyDictionary<Guid, TenderMatch> existingByTenderId,
        TenderMatchCandidate candidate,
        CompanyMatchProfile profile)
    {
        var score = _scorer.Score(candidate, profile);
        var now = _clock.UtcNow;

        if (existingByTenderId.TryGetValue(candidate.TenderId, out var match))
        {
            match.Score = score.Value;
            match.Reason = score.Reason;
            match.ComputedAt = now;
            return;
        }

        _db.TenderMatches.Add(NewMatch(candidate.TenderId, profile.CompanyId, score, now));
    }

    private static TenderMatch NewMatch(Guid tenderId, Guid companyId, TenderMatchScore score, DateTimeOffset now) =>
        new()
        {
            TenderId = tenderId,
            CompanyId = companyId,
            Score = score.Value,
            Reason = score.Reason,
            ComputedAt = now
        };

    private IQueryable<TenderMatchCandidate> OpenTenders()
    {
        var today = _clock.Today;

        return _db.Tenders
            .AsNoTracking()
            .Where(t => t.Status == TenderStatus.Published && t.ClosingOn >= today)
            .Select(t => new TenderMatchCandidate(t.Id, t.Title, t.CategoryLabel, t.GovernorateId));
    }

    private Task<CompanyMatchProfile?> LoadProfileAsync(Guid companyId, CancellationToken cancellationToken) =>
        _db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new CompanyMatchProfile(
                c.Id,
                c.Sectors.Select(s => s.Sector.NameEn).ToList(),
                c.GovernorateId,
                c.Governorate == null ? null : c.Governorate.NameEn))
            .FirstOrDefaultAsync(cancellationToken);
}
