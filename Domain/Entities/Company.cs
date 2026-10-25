namespace Munaqasat.Web.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";

    public string Slug { get; set; } = "";

    public string Initials { get; set; } = "";

    public string? CommercialRegistrationNumber { get; set; }

    public string Address { get; set; } = "";
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string Bio { get; set; } = "";
    public string BioAr { get; set; } = "";

    public Guid? GovernorateId { get; set; }
    public Governorate? Governorate { get; set; }

    public CompanyVerificationStatus VerificationStatus { get; set; } = CompanyVerificationStatus.Pending;
    public DateTimeOffset? VerifiedAt { get; set; }

    public double RatingAverage { get; set; }
    public int RatingCount { get; set; }

    public ICollection<AppUser> Members { get; set; } = new List<AppUser>();
    public ICollection<CompanySector> Sectors { get; set; } = new List<CompanySector>();
    public ICollection<CompanyProfileSection> ProfileSections { get; set; } = new List<CompanyProfileSection>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
}

public class CompanySector
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid SectorId { get; set; }
    public Sector Sector { get; set; } = null!;
}

public class CompanyProfileSection : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Tab { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
}
