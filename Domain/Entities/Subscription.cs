namespace Munaqasat.Web.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";
    public string Description { get; set; } = "";

    public decimal? MonthlyPrice { get; set; }
    public string Currency { get; set; } = "OMR";

    public string PriceLabel { get; set; } = "";

    public int? MonthlyBidLimit { get; set; }

    public bool IsHighlighted { get; set; }
    public int SortOrder { get; set; }

    public ICollection<SubscriptionPlanFeature> Features { get; set; } = new List<SubscriptionPlanFeature>();
    public ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
}

public class SubscriptionPlanFeature : BaseEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    public string Label { get; set; } = "";
    public string LabelAr { get; set; } = "";
    public int SortOrder { get; set; }
}

public class CompanySubscription : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;

    public DateOnly StartsOn { get; set; }

    public DateOnly? RenewsOn { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public bool IsCurrent => Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing;
}
