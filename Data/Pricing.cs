using Munaqasat.Web.Models;

namespace Munaqasat.Web.Data;

/// <summary>Pricing tiers shown on the landing page and in Settings. Not part of the ERD, so they live in code.</summary>
public static class Pricing
{
    public static readonly List<Tier> Tiers = new()
    {
        new(){ Name="Starter", Price="0", Desc="For occasional bidders trying the platform",
            Features = new(){"Browse all open tenders","1 active bid request","Basic company profile","Email support"}, Cta="Start free" },
        new(){ Name="Professional", Price="29", Desc="For contractors actively bidding every month",
            Features = new(){"Everything in Starter","Unlimited bid requests","Match score & smart alerts","Verified profile badge","Priority support"}, Cta="Start free trial", Highlight=true },
        new(){ Name="Enterprise", Price="Custom", Desc="For government entities and large issuers",
            Features = new(){"Everything in Professional","Unlimited tender postings","Team seats & approvals","Dedicated account manager","SLA & onboarding support"}, Cta="Talk to sales" }
    };
}
