using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Bids;

public sealed class BidService : IBidService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public BidService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<Bid>> ListForTenderAsync(
        Guid tenderId,
        CancellationToken cancellationToken = default) =>
        await _db.Bids
            .AsNoTracking()
            .Include(b => b.Company)
            .Where(b => b.TenderId == tenderId)
            .OrderByDescending(b => b.SubmittedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Bid>> ListForCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await _db.Bids
            .AsNoTracking()
            .Include(b => b.Tender)
            .Where(b => b.CompanyId == companyId)
            .OrderByDescending(b => b.SubmittedAt)
            .ToListAsync(cancellationToken);

    public async Task<OperationResult<Guid>> SubmitAsync(
        Guid tenderId,
        BidInput input,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
            return OperationResult<Guid>.Failure("You must be signed in to bid.");

        if (_currentUser.CompanyId is not { } companyId)
            return OperationResult<Guid>.Failure("Only a company account can submit a bid.");

        if (input.Price <= 0)
            return OperationResult<Guid>.Failure("Enter a quoted price greater than zero.");

        if (input.DurationDays <= 0)
            return OperationResult<Guid>.Failure("Enter the delivery period in days.");

        var tender = await _db.Tenders
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenderId, cancellationToken);

        if (tender is null)
            return OperationResult<Guid>.Failure("Tender not found.");

        if (!tender.IsOpenOn(_clock.Today))
            return OperationResult<Guid>.Failure("This tender is no longer open for bids.");

        if (tender.OwnerCompanyId == companyId)
            return OperationResult<Guid>.Failure("You cannot bid on a tender your own company issued.");

        var company = await _db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);

        if (company is null)
            return OperationResult<Guid>.Failure("Company not found.");

        if (company.VerificationStatus != CompanyVerificationStatus.Verified)
            return OperationResult<Guid>.Failure("Your company must be verified before bidding.");

        var hasLiveBid = await _db.Bids.AnyAsync(
            b => b.TenderId == tenderId
                 && b.CompanyId == companyId
                 && (b.Status == BidStatus.Pending || b.Status == BidStatus.Accepted),
            cancellationToken);

        if (hasLiveBid)
            return OperationResult<Guid>.Failure("Your company already has a live bid on this tender. Withdraw it first to re-bid.");

        var bid = new Bid
        {
            TenderId = tenderId,
            CompanyId = companyId,
            SubmittedByUserId = userId,
            Price = input.Price,
            Currency = input.Currency,
            DurationDays = input.DurationDays,
            Notes = input.Notes.Trim(),
            Status = BidStatus.Pending,
            SubmittedAt = _clock.UtcNow
        };

        _db.Bids.Add(bid);
        await _db.SaveChangesAsync(cancellationToken);

        return OperationResult<Guid>.Success(bid.Id);
    }

    public async Task<OperationResult> AcceptAsync(
        Guid bidId,
        string? decisionNote = null,
        CancellationToken cancellationToken = default)
    {
        var bid = await _db.Bids
            .Include(b => b.Tender)
            .FirstOrDefaultAsync(b => b.Id == bidId, cancellationToken);

        if (bid is null) return OperationResult.Failure("Bid not found.");
        if (!CanDecide(bid)) return OperationResult.Failure("You cannot decide on this bid.");
        if (bid.Status != BidStatus.Pending) return OperationResult.Failure("Only a pending bid can be accepted.");

        var now = _clock.UtcNow;

        bid.Status = BidStatus.Accepted;
        bid.DecidedAt = now;
        bid.DecisionNote = decisionNote;

        var others = await _db.Bids
            .Where(b => b.TenderId == bid.TenderId && b.Id != bid.Id && b.Status == BidStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var other in others)
        {
            other.Status = BidStatus.Rejected;
            other.DecidedAt = now;
            other.DecisionNote = "The tender was awarded to another bidder.";
        }

        bid.Tender.Status = TenderStatus.Awarded;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task<OperationResult> RejectAsync(
        Guid bidId,
        string? decisionNote = null,
        CancellationToken cancellationToken = default)
    {
        var bid = await _db.Bids
            .Include(b => b.Tender)
            .FirstOrDefaultAsync(b => b.Id == bidId, cancellationToken);

        if (bid is null) return OperationResult.Failure("Bid not found.");
        if (!CanDecide(bid)) return OperationResult.Failure("You cannot decide on this bid.");
        if (bid.Status != BidStatus.Pending) return OperationResult.Failure("Only a pending bid can be rejected.");

        bid.Status = BidStatus.Rejected;
        bid.DecidedAt = _clock.UtcNow;
        bid.DecisionNote = decisionNote;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    public async Task<OperationResult> WithdrawAsync(Guid bidId, CancellationToken cancellationToken = default)
    {
        var bid = await _db.Bids.FirstOrDefaultAsync(b => b.Id == bidId, cancellationToken);

        if (bid is null) return OperationResult.Failure("Bid not found.");

        if (bid.CompanyId != _currentUser.CompanyId)
            return OperationResult.Failure("Only the bidding company can withdraw this bid.");

        if (bid.Status != BidStatus.Pending)
            return OperationResult.Failure("Only a pending bid can be withdrawn.");

        bid.Status = BidStatus.Withdrawn;
        bid.DecidedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    private bool CanDecide(Bid bid) =>
        _currentUser.IsAdministrator ||
        (bid.Tender.OwnerCompanyId is { } ownerId && ownerId == _currentUser.CompanyId);
}
