using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Companies;

namespace Munaqasat.Web.Services.Reviews;

public sealed class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyService _companies;

    public ReviewService(ApplicationDbContext db, ICurrentUser currentUser, ICompanyService companies)
    {
        _db = db;
        _currentUser = currentUser;
        _companies = companies;
    }

    public async Task<IReadOnlyList<Review>> ListForCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await _db.Reviews
            .AsNoTracking()
            .Include(r => r.AuthorUser)
            .Include(r => r.Tender)
            .Where(r => r.CompanyId == companyId && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<OperationResult<Guid>> AddAsync(
        Guid companyId,
        ReviewInput input,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } authorUserId)
            return OperationResult<Guid>.Failure("You must be signed in to leave a review.");

        if (_currentUser.CompanyId == companyId)
            return OperationResult<Guid>.Failure("You cannot review your own company.");

        if (OutOfRange(input) is { } rangeError)
            return OperationResult<Guid>.Failure(rangeError);

        var companyExists = await _db.Companies.AnyAsync(c => c.Id == companyId, cancellationToken);
        if (!companyExists) return OperationResult<Guid>.Failure("Company not found.");

        var alreadyReviewed = await _db.Reviews
            .AnyAsync(r => r.CompanyId == companyId && r.AuthorUserId == authorUserId, cancellationToken);

        if (alreadyReviewed)
            return OperationResult<Guid>.Failure("You have already reviewed this company.");

        var review = new Review
        {
            CompanyId = companyId,
            AuthorUserId = authorUserId,
            TenderId = input.TenderId,
            QualityRating = input.QualityRating,
            CommunicationRating = input.CommunicationRating,
            ExperienceRating = input.ExperienceRating,
            DeadlineRating = input.DeadlineRating,
            Comment = input.Comment.Trim(),
            IsAnonymous = input.IsAnonymous,
            IsPublished = true
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);

        await _companies.RecalculateRatingAsync(companyId, cancellationToken);

        return OperationResult<Guid>.Success(review.Id);
    }

    public async Task<OperationResult> SetPublishedAsync(
        Guid reviewId,
        bool isPublished,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdministrator)
            return OperationResult.Failure("Only an administrator can moderate reviews.");

        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
        if (review is null) return OperationResult.Failure("Review not found.");

        review.IsPublished = isPublished;
        await _db.SaveChangesAsync(cancellationToken);

        await _companies.RecalculateRatingAsync(review.CompanyId, cancellationToken);

        return OperationResult.Success();
    }

    private static string? OutOfRange(ReviewInput input)
    {
        var scores = new[]
        {
            input.QualityRating,
            input.CommunicationRating,
            input.ExperienceRating,
            input.DeadlineRating
        };

        return scores.Any(score => score is < 1 or > 5)
            ? "Every rating must be between 1 and 5."
            : null;
    }
}
