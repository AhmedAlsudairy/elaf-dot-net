namespace Munaqasat.Web.Domain;

public enum AccountType
{
    Individual = 0,
    Company = 1
}

public enum TenderCategory
{
    Supply = 0,
    Services = 1,
    Works = 2
}

public enum TenderStatus
{
    Draft = 0,
    Published = 1,
    Closed = 2,
    Awarded = 3,
    Cancelled = 4
}

public enum BidStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Withdrawn = 3
}

public enum CompanyVerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
    Suspended = 3
}

public enum SubscriptionStatus
{
    Trialing = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
    Expired = 4
}
