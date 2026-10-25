using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Bids;

public sealed class BidInput
{
    public decimal Price { get; set; }
    public string Currency { get; set; } = "OMR";
    public int DurationDays { get; set; }
    public string Notes { get; set; } = "";
}

public interface IBidService
{
    Task<IReadOnlyList<Bid>> ListForTenderAsync(Guid tenderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Bid>> ListForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> SubmitAsync(Guid tenderId, BidInput input, CancellationToken cancellationToken = default);

    Task<OperationResult> AcceptAsync(Guid bidId, string? decisionNote = null, CancellationToken cancellationToken = default);

    Task<OperationResult> RejectAsync(Guid bidId, string? decisionNote = null, CancellationToken cancellationToken = default);

    Task<OperationResult> WithdrawAsync(Guid bidId, CancellationToken cancellationToken = default);
}
