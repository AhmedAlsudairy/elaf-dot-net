using Munaqasat.Web.Domain;

namespace Munaqasat.Web.Services.Tenders;

public sealed record TenderListItem
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string Title { get; init; }
    public required string TitleAr { get; init; }
    public required string IssuingEntity { get; init; }
    public required TenderCategory Category { get; init; }
    public required string CategoryLabel { get; init; }
    public string? GovernorateName { get; init; }
    public string? WilayatName { get; init; }
    public decimal? BudgetMin { get; init; }
    public decimal? BudgetMax { get; init; }
    public required string Currency { get; init; }
    public DateOnly? PublishedOn { get; init; }
    public required DateOnly ClosingOn { get; init; }
    public required TenderStatus Status { get; init; }
    public required string Icon { get; init; }
    public required int BidCount { get; init; }

    public int? MatchScore { get; init; }

    public int DaysUntilClosing(DateOnly today) => ClosingOn.DayNumber - today.DayNumber;
}

public sealed class TenderInput
{
    public string Reference { get; set; } = "";
    public string Title { get; set; } = "";
    public string TitleAr { get; set; } = "";
    public string Description { get; set; } = "";
    public string DescriptionAr { get; set; } = "";
    public string IssuingEntity { get; set; } = "";
    public TenderCategory Category { get; set; }
    public string CategoryLabel { get; set; } = "";
    public Guid? GovernorateId { get; set; }
    public Guid? WilayatId { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string Currency { get; set; } = "OMR";
    public decimal? DocumentFee { get; set; }
    public decimal? BidBondPercentage { get; set; }
    public DateOnly ClosingOn { get; set; }
    public string Icon { get; set; } = "i-box";

    public List<TenderCustomFieldInput> CustomFields { get; set; } = new();
}

public sealed class TenderCustomFieldInput
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}
