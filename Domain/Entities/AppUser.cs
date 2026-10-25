using Microsoft.AspNetCore.Identity;

namespace Munaqasat.Web.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = "";
    public AccountType AccountType { get; set; } = AccountType.Individual;

    public string PreferredLanguage { get; set; } = "en";

    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastSignedInAt { get; set; }

    public ICollection<Bid> SubmittedBids { get; set; } = new List<Bid>();
    public ICollection<Review> AuthoredReviews { get; set; } = new List<Review>();
}

public class AppRole : IdentityRole<Guid>
{
    public string Description { get; set; } = "";

    public AppRole() { }

    public AppRole(string name, string description) : base(name) => Description = description;
}

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string CompanyOwner = "CompanyOwner";
    public const string CompanyMember = "CompanyMember";
    public const string Individual = "Individual";

    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        [Administrator] = "Full access to the admin console, verification queues and platform settings.",
        [CompanyOwner] = "Manages a company profile, its members and the bids it submits.",
        [CompanyMember] = "Submits and tracks bids on behalf of a company.",
        [Individual] = "Browses tenders and reviews companies without acting for an organisation."
    };
}
