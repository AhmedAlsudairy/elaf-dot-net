using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Reviews;

public sealed class ReviewInput
{
    public int QualityRating { get; set; }
    public int CommunicationRating { get; set; }
    public int ExperienceRating { get; set; }
    public int DeadlineRating { get; set; }
    public string Comment { get; set; } = "";
    public bool IsAnonymous { get; set; }

    public Guid? TenderId { get; set; }
}

public interface IReviewService
{
    Task<IReadOnlyList<Review>> ListForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> AddAsync(Guid companyId, ReviewInput input, CancellationToken cancellationToken = default);

    Task<OperationResult> SetPublishedAsync(Guid reviewId, bool isPublished, CancellationToken cancellationToken = default);
}
