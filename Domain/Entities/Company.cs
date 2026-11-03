namespace Munaqasat.Web.Domain.Entities;

public class Company : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string? CompanyProfileId { get; set; }
    public string? CompanyNumber { get; set; }
    public string? CompanyWebsite { get; set; }
    public string CompanyEmail { get; set; } = "";
    public List<string>? Sectors { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
    public string DefaultLanguage { get; set; } = "en";
    public Language DefaultLanguageNavigation { get; set; } = null!;

    // Cached aggregates, maintained by the company_ratings trigger.
    public double AvgCommunication { get; set; }
    public double AvgDeadline { get; set; }
    public double AvgExperience { get; set; }
    public double AvgOverallRating { get; set; }
    public double AvgQuality { get; set; }
    public int NumberOfRatings { get; set; }

    // Addition to the ERD for the admin console's Verified / Pending review column.
    public string VerificationStatus { get; set; } = VerificationStatuses.Pending;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<CompanyTranslation> Translations { get; set; } = new List<CompanyTranslation>();
    public ICollection<CustomSection> CustomSections { get; set; } = new List<CustomSection>();
    public ICollection<UserProfile> Users { get; set; } = new List<UserProfile>();
    public ICollection<TenderRequest> Requests { get; set; } = new List<TenderRequest>();
    public ICollection<CompanyRating> RatingsReceived { get; set; } = new List<CompanyRating>();
    public ICollection<CompanyRating> RatingsGiven { get; set; } = new List<CompanyRating>();
}

public class CompanyTranslation : IHasTimestamps, ITranslation
{
    public string Id { get; set; } = Ids.New();
    public string CompanyId { get; set; } = "";
    public Company Company { get; set; } = null!;
    public string LanguageCode { get; set; } = "";
    public Language Language { get; set; } = null!;
    public string CompanyTitle { get; set; } = "";
    public string? Bio { get; set; }
    public string? Address { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CustomSection : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string CompanyProfileId { get; set; } = "";
    public Company Company { get; set; } = null!;
    public string? FileUrl { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<CustomSectionTranslation> Translations { get; set; } = new List<CustomSectionTranslation>();
}

public class CustomSectionTranslation : ITranslation
{
    public string Id { get; set; } = Ids.New();
    public string CustomSectionId { get; set; } = "";
    public CustomSection CustomSection { get; set; } = null!;
    public string LanguageCode { get; set; } = "";
    public Language Language { get; set; } = null!;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string TabName { get; set; } = "";
}

public class UserProfile : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ProfileImage { get; set; }
    public string? Role { get; set; }
    public string? CompanyThatWorkedWith { get; set; }

    /// <summary>
    /// The auth provider's user id. Named after the ERD (Clerk); today it holds the
    /// ASP.NET Core Identity user id, so switching to Clerk later only changes what is stored here.
    /// </summary>
    public string ClerkUserId { get; set; } = "";

    public string? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public Language PreferredLanguageNavigation { get; set; } = null!;
    public string Timezone { get; set; } = "Asia/Muscat";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
