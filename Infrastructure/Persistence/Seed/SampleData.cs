using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services;

namespace Munaqasat.Web.Infrastructure.Persistence.Seed;

/// <summary>
/// The sample content the frontend was designed with (formerly Data/MockData.cs), stored in the ERD tables.
/// Development only: enabled by <c>Seed:SampleData</c>. Skipped if tender "t1" already exists.
/// </summary>
internal static class SampleData
{
    private record TenderSeed(
        string Id, string Entity, string Title, string TenderNo, string ExtId, string Category, string SubCategory,
        string Governorate, string Wilayat, decimal BudgetMin, decimal BudgetMax, DateOnly Published, DateOnly Closing,
        decimal DocFee, decimal Bond, string Description, (string Title, string Description)[] Fields);

    private record CompanySeed(
        string Id, string Name, string Email, string? Website, string? Address, string[] Sectors, bool Verified,
        string? Bio = null, (string Tab, string Title, string Description)[]? Sections = null);

    private static readonly TenderSeed[] Tenders =
    {
        new("t1", "Ministry of Transport, Communications and IT", "Supply and Installation of Network Infrastructure Equipment",
            "112/2026/MTCIT/DGIT-14", "98629", "SUPPLY", "Technology", "Muscat", "Bawshar", 190000, 385000,
            new(2026, 8, 20), new(2026, 10, 15), 25, 2.0m,
            "Procurement, delivery and installation of core and access-layer networking equipment across three regional data centres, including a 3-year onsite maintenance contract and staff handover training.",
            new[] { ("Warranty period", "Minimum 3 years onsite manufacturer warranty on all supplied hardware."),
                    ("Local content", "A minimum of 35% Omanisation among assigned technical staff is required.") }),
        new("t2", "Public Authority for Special Economic Zones (OPAZ)", "Digital Transformation Advisory & ERP Implementation Consulting",
            "203/2026/OPAZ/DGP-08", "99214", "SERVICES", "Consulting", "Al Wusta", "Duqm", 77000, 192000,
            new(2026, 8, 5), new(2026, 11, 2), 15, 2.5m,
            "Advisory engagement to assess current ERP readiness and lead a phased implementation across finance, procurement and HR modules for the Duqm Special Economic Zone administration.",
            new[] { ("Team composition", "Proposal must name a lead consultant with at least 8 years of ERP implementation experience.") }),
        new("t3", "Ministry of Finance", "Cloud Migration and Managed Hosting Services for Government Data Centre",
            "355/2026/MOF/DGIS-21", "99871", "SERVICES", "Cloud", "Muscat", "Seeb", 385000, 770000,
            new(2026, 8, 12), new(2026, 10, 28), 30, 3.0m,
            "Migration of legacy on-premise workloads to a hybrid cloud environment with managed hosting, backup and 24/7 monitoring under a 3-year service agreement.",
            new[] { ("Data residency", "All citizen data must remain hosted within Omani territory."),
                    ("SLA", "99.9% monthly uptime with financial penalties for breaches.") }),
        new("t4", "Ministry of Transport", "Construction of Access Roads — Wilayat Ibra Phase 2",
            "090/2026/MOT/RDS-19", "97650", "WORKS", "Roads & Infrastructure", "North Sharqiyah", "Ibra", 1200000, 2500000,
            new(2026, 7, 28), new(2026, 9, 20), 50, 5.0m,
            "Construction of 14km of asphalt access roads including drainage works, street lighting and signage, connecting three residential areas to the main Ibra–Sur highway.",
            new[] { ("Grade requirement", "Contractor must hold a valid Grade 1 or Grade 2 civil works classification.") }),
        new("t5", "Ministry of Health", "Annual Maintenance Contract for HVAC Systems — Royal Hospital",
            "128/2026/MOH/ENG-33", "98042", "SERVICES", "Facilities Management", "Muscat", "Bawshar", 65000, 140000,
            new(2026, 9, 1), new(2026, 11, 10), 10, 1.5m,
            "Preventive and reactive maintenance of all HVAC systems across hospital wards, operating theatres and administrative buildings under a 12-month renewable contract.",
            new[] { ("Response time", "Critical faults in operating theatres must be attended within 2 hours, 24/7.") }),
        new("t6", "Ministry of Labour", "Supply of Laboratory Equipment for Vocational Colleges",
            "071/2026/MOL/PROC-05", "96410", "SUPPLY", "Education & Training", "Al Batinah North", "Sohar", 140000, 260000,
            new(2026, 8, 25), new(2026, 10, 5), 20, 2.0m,
            "Supply, delivery and installation of electronics, mechanical and materials-testing laboratory equipment across four vocational training colleges, with staff training included.",
            new[] { ("Delivery schedule", "All equipment must be delivered and commissioned before the start of the academic term.") })
    };

    private static readonly CompanySeed[] Companies =
    {
        new("c1", "Gulf Systems Integration LLC", "info@gulfsystems.om", "https://gulfsystems.om", "Al Khuwair, Muscat, Oman",
            new[] { "Technology", "Networking", "Cloud" }, true,
            "Gulf Systems Integration LLC has delivered enterprise networking, cloud and systems-integration projects for government and private-sector clients across Oman since 2011. The team holds Grade 1 IT classification and has completed over 60 public-sector contracts.",
            new[] { ("Certifications", "ISO & vendor certifications", "ISO 27001:2022 certified. Cisco Gold Partner, Microsoft Solutions Partner for Infrastructure."),
                    ("Portfolio", "Selected past work", "Network refresh for 40+ government branch offices (2024); national data-centre network redesign (2023); managed SD-WAN rollout for a regional bank (2022).") }),
        new("c2", "Al Nahda Consulting Group", "info@alnahdaconsulting.om", "https://alnahdaconsulting.om", "Ruwi, Muscat, Oman",
            new[] { "Consulting", "ERP", "Change Management" }, true,
            "Al Nahda Consulting Group advises public authorities on digital transformation, ERP selection and process re-engineering, with a track record across free-zone and municipal authorities.",
            new[] { ("Certifications", "Accreditations", "SAP Certified Partner. PMI-registered project management practice.") }),
        new("c3", "CloudPeak Technologies LLC", "info@cloudpeak.om", null, "Muscat, Oman", new[] { "Cloud", "Cybersecurity" }, false),
        new("c4", "Bawshar Technical Services", "info@bawshartech.om", null, "Bawshar, Muscat, Oman", new[] { "Facilities", "HVAC" }, true),
        new("c5", "Sohar Scientific Supplies", "info@soharscientific.om", null, "Sohar, Al Batinah North, Oman", new[] { "Lab Equipment", "Supply" }, true),
        new("c6", "Al Falaj Networks Co.", "info@alfalajnetworks.om", null, "Muscat, Oman", new[] { "Networking", "Technology" }, true),
        new("c7", "Comfort Air Solutions LLC", "info@comfortair.om", null, "Muscat, Oman", new[] { "HVAC", "Facilities" }, true),
        // Issuing organisations, so they can rate the contractors they worked with.
        new("g1", "Ministry of Transport, Communications and IT", "procurement@mtcit.gov.om", "https://mtcit.gov.om", "Muscat, Oman", Array.Empty<string>(), true),
        new("g2", "Public Authority for Special Economic Zones", "procurement@opaz.gov.om", "https://opaz.gov.om", "Duqm, Al Wusta, Oman", Array.Empty<string>(), true),
        new("g3", "Ministry of Finance", "procurement@mof.gov.om", "https://mof.gov.om", "Muscat, Oman", Array.Empty<string>(), true)
    };

    private static readonly (string Id, string Tender, string Company, decimal Price, int Days, RequestStatus Status, DateOnly Date)[] Requests =
    {
        ("r1", "t1", "c1", 342500, 120, RequestStatus.Accepted, new(2026, 9, 2)),
        ("r2", "t1", "c6", 359900, 110, RequestStatus.Rejected, new(2026, 9, 1)),
        ("r3", "t2", "c2", 168000, 180, RequestStatus.Pending, new(2026, 9, 12)),
        ("r4", "t3", "c3", 610000, 90, RequestStatus.Pending, new(2026, 9, 18)),
        ("r5", "t5", "c4", 98500, 365, RequestStatus.Pending, new(2026, 9, 20)),
        ("r6", "t5", "c7", 112000, 365, RequestStatus.Pending, new(2026, 9, 19)),
        ("r7", "t6", "c5", 225000, 60, RequestStatus.Withdrawn, new(2026, 8, 30))
    };

    private static readonly (string Rater, string Rated, string Request, int Q, int C, int E, int D, string Comment, bool Anonymous, DateOnly Date)[] Ratings =
    {
        ("g1", "c1", "r1", 5, 5, 4, 4, "Delivered the network refresh two days ahead of schedule with clear weekly status updates.", false, new(2026, 6, 2)),
        ("g3", "c1", "r1", 4, 5, 5, 4, "Strong technical team. Documentation could have been slightly more detailed at handover.", true, new(2026, 4, 18)),
        ("g2", "c2", "r3", 4, 4, 5, 4, "Solid advisory work; the phased rollout plan reduced disruption significantly.", false, new(2026, 5, 10))
    };

    private static readonly (string Name, string Email, string Role, string? Company, DateOnly Joined)[] Users =
    {
        ("Ahmed Al Rawahi", "ahmed@gulfsystems.om", "client", "c1", new(2025, 2, 14)),
        ("Layla Al Hinai", "layla@alnahda.om", "contractor", "c2", new(2025, 5, 3)),
        ("Yousuf Al Balushi", "yousuf@mtcit.gov.om", "client", "g1", new(2024, 11, 20)),
        ("Maha Al Kindi", "maha@cloudpeak.om", "contractor", "c3", new(2026, 1, 9))
    };

    public static async Task SeedAsync(ApplicationDbContext db, UserManager<IdentityUser> users, ILogger log)
    {
        if (await db.Tenders.AnyAsync(t => t.Id == "t1"))
        {
            log.LogInformation("Sample data already present");
            return;
        }

        foreach (var s in Tenders)
        {
            var tender = new Tender
            {
                Id = s.Id,
                ExternalTenderId = s.ExtId,
                TenderNo = s.TenderNo,
                ProcurementCategoryCode = s.Category,
                TenderTypeCode = "OPEN_LOCAL",
                EvaluationTypeCode = "ITEM_WISE",
                EnvelopeTypeCode = "SINGLE",
                PrebidClarificationMode = "ONLINE",
                DocumentFee = s.DocFee,
                DocumentFeePaymentMode = "ONLINE_PAYMENT",
                BondPercentage = s.Bond,
                IsEBondApplicable = true,
                OriginalLanguage = "en",
                PublishedDate = s.Published,
                BidSubmissionClosing = AtNoonMuscat(s.Closing),
                BudgetMin = s.BudgetMin,
                BudgetMax = s.BudgetMax,
                Status = TenderStatuses.Published
            };

            tender.Translations.Add(new TenderTranslation
            {
                LanguageCode = "en",
                EntityName = s.Entity,
                Title = s.Title,
                ProcurementCategory = s.Category[..1] + s.Category[1..].ToLowerInvariant(),
                ProcurementSubCategory = s.SubCategory,
                Governorate = s.Governorate,
                Wilayat = s.Wilayat,
                Note = s.Description
            });

            var order = 0;
            foreach (var (title, description) in s.Fields)
            {
                var field = new CustomField { SortOrder = order++ };
                field.Translations.Add(new CustomFieldTranslation { LanguageCode = "en", Title = title, Description = description });
                tender.CustomFields.Add(field);
            }

            db.Tenders.Add(tender);
        }

        // One Arabic translation so the fallback rule (requested → record default → en) is visible with ?lang=ar.
        db.Tenders.Local.First(t => t.Id == "t1").Translations.Add(new TenderTranslation
        {
            LanguageCode = "ar",
            EntityName = "وزارة النقل والاتصالات وتقنية المعلومات",
            Title = "توريد وتركيب معدات البنية التحتية للشبكات",
            ProcurementCategory = "توريد",
            ProcurementSubCategory = "التقنية",
            Governorate = "مسقط",
            Wilayat = "بوشر",
            Note = "شراء وتوريد وتركيب معدات الشبكات الأساسية وطبقة الوصول في ثلاثة مراكز بيانات إقليمية، بما في ذلك عقد صيانة في الموقع لمدة 3 سنوات وتدريب الموظفين.",
            IsMachineTranslated = true
        });

        foreach (var s in Companies)
        {
            var company = new Company
            {
                Id = s.Id,
                CompanyEmail = s.Email,
                CompanyWebsite = s.Website,
                Sectors = s.Sectors.ToList(),
                DefaultLanguage = "en",
                VerificationStatus = s.Verified ? VerificationStatuses.Verified : VerificationStatuses.Pending
            };
            company.Translations.Add(new CompanyTranslation { LanguageCode = "en", CompanyTitle = s.Name, Address = s.Address, Bio = s.Bio });

            var order = 0;
            foreach (var (tab, title, description) in s.Sections ?? Array.Empty<(string, string, string)>())
            {
                var section = new CustomSection { SortOrder = order++ };
                section.Translations.Add(new CustomSectionTranslation { LanguageCode = "en", TabName = tab, Title = title, Description = description });
                company.CustomSections.Add(section);
            }

            db.Companies.Add(company);
        }

        db.Companies.Local.First(c => c.Id == "c1").Translations.Add(new CompanyTranslation
        {
            LanguageCode = "ar",
            CompanyTitle = "الخليج لتكامل الأنظمة ش.م.م",
            Address = "الخوير، مسقط، عُمان",
            Bio = "تنفذ شركة الخليج لتكامل الأنظمة مشاريع الشبكات والحوسبة السحابية وتكامل الأنظمة للجهات الحكومية والخاصة في عُمان منذ عام 2011."
        });

        foreach (var r in Requests)
        {
            db.TenderRequests.Add(new TenderRequest
            {
                Id = r.Id, TenderId = r.Tender, CompanyProfileId = r.Company, Price = r.Price, Days = r.Days,
                Status = r.Status, LanguageCode = "en"
            });
        }

        var ratingIds = new List<(string Id, DateOnly Date)>();
        foreach (var r in Ratings)
        {
            var rating = new CompanyRating
            {
                RatingCompanyId = r.Rater, RatedCompanyId = r.Rated, TenderRequestId = r.Request,
                TenderId = Requests.First(x => x.Id == r.Request).Tender,
                Quality = r.Q, Communication = r.C, Experience = r.E, Deadline = r.D,
                OverallRating = CompanyRating.ComputeOverall(r.Q, r.C, r.E, r.D),
                Comment = r.Comment, Anonymous = r.Anonymous, LanguageCode = "en"
            };
            db.CompanyRatings.Add(rating);
            ratingIds.Add((rating.Id, r.Date));
        }

        foreach (var u in Users)
        {
            var authUser = new IdentityUser { UserName = u.Email, Email = u.Email, EmailConfirmed = true };
            // No password: these accounts show in the admin console but cannot sign in.
            var created = await users.CreateAsync(authUser);
            if (!created.Succeeded) throw new InvalidOperationException(string.Join(" ", created.Errors.Select(e => e.Description)));
            await users.AddToRoleAsync(authUser, Munaqasat.Web.Services.Accounts.AccountService.RoleByKey[u.Role]);

            db.UserProfiles.Add(new UserProfile
            {
                Name = u.Name, Email = u.Email, Role = u.Role, CompanyId = u.Company, IdentityUserId = authUser.Id
            });
        }

        await db.SaveChangesAsync();

        // created_at is stamped on insert; backdate the sample rows to the dates the UI was designed with.
        foreach (var r in Requests)
        {
            var at = AtNoonMuscat(r.Date);
            await db.TenderRequests.Where(x => x.Id == r.Id).ExecuteUpdateAsync(x => x.SetProperty(p => p.CreatedAt, at));
        }

        foreach (var (id, date) in ratingIds)
        {
            var at = AtNoonMuscat(date);
            await db.CompanyRatings.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(p => p.CreatedAt, at));
        }

        foreach (var u in Users)
        {
            var at = AtNoonMuscat(u.Joined);
            await db.UserProfiles.Where(x => x.Email == u.Email).ExecuteUpdateAsync(x => x.SetProperty(p => p.CreatedAt, at));
        }

        log.LogInformation("Seeded sample tenders, companies, requests, ratings and users");
    }

    private static DateTimeOffset AtNoonMuscat(DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(new TimeOnly(12, 0)), SystemClock.OmanOffset).ToUniversalTime();
}
