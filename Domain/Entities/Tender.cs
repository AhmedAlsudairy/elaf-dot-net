namespace Munaqasat.Web.Domain.Entities;

public class Tender : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string ExternalTenderId { get; set; } = "";
    public string TenderNo { get; set; } = "";
    public string ProcurementCategoryCode { get; set; } = "";
    public string TenderTypeCode { get; set; } = "";
    public string? EvaluationTypeCode { get; set; }
    public string? EnvelopeTypeCode { get; set; }
    public bool IsAlternateBidAllowed { get; set; }
    public List<string>? VendorGrades { get; set; }
    public string? PrebidClarificationMode { get; set; }
    public decimal? DocumentFee { get; set; }
    public string? DocumentFeePaymentMode { get; set; }
    public decimal? BondPercentage { get; set; }
    public bool IsEBondApplicable { get; set; }
    public string OriginalLanguage { get; set; } = "en";
    public Language OriginalLanguageNavigation { get; set; } = null!;
    public DateOnly PublishedDate { get; set; }
    public DateTimeOffset? DocumentSalesStart { get; set; }
    public DateTimeOffset? DocumentSalesEnd { get; set; }
    public DateTimeOffset? PrebidClarificationStart { get; set; }
    public DateTimeOffset? PrebidClarificationEnd { get; set; }
    public DateTimeOffset BidSubmissionClosing { get; set; }
    public DateTimeOffset? PricedBidOpening { get; set; }

    // Additions to the ERD, needed by the frontend (budget chips/filter, draft vs published).
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string Status { get; set; } = TenderStatuses.Published;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TenderTranslation> Translations { get; set; } = new List<TenderTranslation>();
    public ICollection<CustomField> CustomFields { get; set; } = new List<CustomField>();
    public ICollection<TenderRequest> Requests { get; set; } = new List<TenderRequest>();
}

public class TenderTranslation : IHasTimestamps, ITranslation
{
    public string Id { get; set; } = Ids.New();
    public string TenderId { get; set; } = "";
    public Tender Tender { get; set; } = null!;
    public string LanguageCode { get; set; } = "";
    public Language Language { get; set; } = null!;
    public string EntityName { get; set; } = "";
    public string Title { get; set; } = "";
    public string? ProcurementCategory { get; set; }
    public string? ProcurementSubCategory { get; set; }
    public string? Governorate { get; set; }
    public string? Wilayat { get; set; }
    public string? Note { get; set; }
    public bool IsMachineTranslated { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CustomField : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string TenderId { get; set; } = "";
    public Tender Tender { get; set; } = null!;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<CustomFieldTranslation> Translations { get; set; } = new List<CustomFieldTranslation>();
}

public class CustomFieldTranslation : ITranslation
{
    public string Id { get; set; } = Ids.New();
    public string CustomFieldId { get; set; } = "";
    public CustomField CustomField { get; set; } = null!;
    public string LanguageCode { get; set; } = "";
    public Language Language { get; set; } = null!;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class TenderRequest : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string TenderId { get; set; } = "";
    public Tender Tender { get; set; } = null!;
    public string CompanyProfileId { get; set; } = "";
    public Company Company { get; set; } = null!;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public decimal? Price { get; set; }
    public int? Days { get; set; }
    public string? Description { get; set; }
    public string? LanguageCode { get; set; }
    public Language? Language { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
