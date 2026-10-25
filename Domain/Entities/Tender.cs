namespace Munaqasat.Web.Domain.Entities;

public class Tender : BaseEntity
{
    public string Reference { get; set; } = "";

    public string? ExternalId { get; set; }

    public string Title { get; set; } = "";
    public string TitleAr { get; set; } = "";
    public string Description { get; set; } = "";
    public string DescriptionAr { get; set; } = "";

    public string IssuingEntity { get; set; } = "";

    public TenderCategory Category { get; set; }

    public string CategoryLabel { get; set; } = "";

    public Guid? GovernorateId { get; set; }
    public Governorate? Governorate { get; set; }

    public Guid? WilayatId { get; set; }
    public Wilayat? Wilayat { get; set; }

    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }

    public string Currency { get; set; } = "OMR";

    public decimal? DocumentFee { get; set; }

    public decimal? BidBondPercentage { get; set; }

    public DateOnly? PublishedOn { get; set; }
    public DateOnly ClosingOn { get; set; }

    public TenderStatus Status { get; set; } = TenderStatus.Draft;

    public Guid? OwnerCompanyId { get; set; }
    public Company? OwnerCompany { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public AppUser? CreatedByUser { get; set; }

    public string Icon { get; set; } = "i-box";

    public ICollection<TenderCustomField> CustomFields { get; set; } = new List<TenderCustomField>();
    public ICollection<TenderAttachment> Attachments { get; set; } = new List<TenderAttachment>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<TenderMatch> Matches { get; set; } = new List<TenderMatch>();

    public bool IsOpenOn(DateOnly today) => Status == TenderStatus.Published && ClosingOn >= today;
}

public class TenderCustomField : BaseEntity
{
    public Guid TenderId { get; set; }
    public Tender Tender { get; set; } = null!;

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
}

public class TenderAttachment : BaseEntity
{
    public Guid TenderId { get; set; }
    public Tender Tender { get; set; } = null!;

    public string FileName { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }

    public Guid? UploadedByUserId { get; set; }
}

public class TenderMatch
{
    public Guid TenderId { get; set; }
    public Tender Tender { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public int Score { get; set; }

    public string Reason { get; set; } = "";

    public DateTimeOffset ComputedAt { get; set; }
}
