namespace Munaqasat.Web.Domain.Entities;

public class Bid : BaseEntity
{
    public Guid TenderId { get; set; }
    public Tender Tender { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid SubmittedByUserId { get; set; }
    public AppUser SubmittedByUser { get; set; } = null!;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "OMR";

    public int DurationDays { get; set; }

    public string Notes { get; set; } = "";

    public BidStatus Status { get; set; } = BidStatus.Pending;

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public string? DecisionNote { get; set; }

    public bool IsLive => Status is BidStatus.Pending or BidStatus.Accepted;
}
