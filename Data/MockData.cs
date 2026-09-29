using Munaqasat.Web.Models;

namespace Munaqasat.Web.Data;

public static class MockData
{
    public static readonly List<Tender> Tenders = new()
    {
        new Tender {
            Id="t1", Entity="Ministry of Transport, Communications and IT",
            Title="Supply and Installation of Network Infrastructure Equipment",
            TenderNo="112/2026/MTCIT/DGIT-14", ExtId="98629", Category="SUPPLY", CategoryLabel="Technology",
            Governorate="Muscat", Wilayat="Bawshar", BudgetLabel="OMR 190K–385K", BudgetMin=190000, BudgetMax=385000,
            Published=new DateOnly(2026,8,20), Closing=new DateOnly(2026,10,15), DocFee="OMR 25.000", Bond="2.0% of quoted value",
            Score=96, IsNew=true, Icon="i-box",
            Description="Procurement, delivery and installation of core and access-layer networking equipment across three regional data centres, including a 3-year onsite maintenance contract and staff handover training.",
            CustomFields = new() {
                new CustomField{Title="Warranty period", Description="Minimum 3 years onsite manufacturer warranty on all supplied hardware."},
                new CustomField{Title="Local content", Description="A minimum of 35% Omanisation among assigned technical staff is required."}
            }
        },
        new Tender {
            Id="t2", Entity="Public Authority for Special Economic Zones (OPAZ)",
            Title="Digital Transformation Advisory & ERP Implementation Consulting",
            TenderNo="203/2026/OPAZ/DGP-08", ExtId="99214", Category="SERVICES", CategoryLabel="Consulting",
            Governorate="Al Wusta", Wilayat="Duqm", BudgetLabel="OMR 77K–192K", BudgetMin=77000, BudgetMax=192000,
            Published=new DateOnly(2026,8,5), Closing=new DateOnly(2026,11,2), DocFee="OMR 15.000", Bond="2.5% of quoted value",
            Score=82, IsNew=false, Icon="i-briefcase",
            Description="Advisory engagement to assess current ERP readiness and lead a phased implementation across finance, procurement and HR modules for the Duqm Special Economic Zone administration.",
            CustomFields = new() {
                new CustomField{Title="Team composition", Description="Proposal must name a lead consultant with at least 8 years of ERP implementation experience."}
            }
        },
        new Tender {
            Id="t3", Entity="Ministry of Finance",
            Title="Cloud Migration and Managed Hosting Services for Government Data Centre",
            TenderNo="355/2026/MOF/DGIS-21", ExtId="99871", Category="SERVICES", CategoryLabel="Cloud",
            Governorate="Muscat", Wilayat="Seeb", BudgetLabel="OMR 385K–770K", BudgetMin=385000, BudgetMax=770000,
            Published=new DateOnly(2026,8,12), Closing=new DateOnly(2026,10,28), DocFee="OMR 30.000", Bond="3.0% of quoted value",
            Score=74, IsNew=false, Icon="i-cloud",
            Description="Migration of legacy on-premise workloads to a hybrid cloud environment with managed hosting, backup and 24/7 monitoring under a 3-year service agreement.",
            CustomFields = new() {
                new CustomField{Title="Data residency", Description="All citizen data must remain hosted within Omani territory."},
                new CustomField{Title="SLA", Description="99.9% monthly uptime with financial penalties for breaches."}
            }
        },
        new Tender {
            Id="t4", Entity="Ministry of Transport",
            Title="Construction of Access Roads — Wilayat Ibra Phase 2",
            TenderNo="090/2026/MOT/RDS-19", ExtId="97650", Category="WORKS", CategoryLabel="Roads & Infrastructure",
            Governorate="North Sharqiyah", Wilayat="Ibra", BudgetLabel="OMR 1.2M–2.5M", BudgetMin=1200000, BudgetMax=2500000,
            Published=new DateOnly(2026,7,28), Closing=new DateOnly(2026,9,20), DocFee="OMR 50.000", Bond="5.0% of quoted value",
            Score=61, IsNew=false, Icon="i-tool",
            Description="Construction of 14km of asphalt access roads including drainage works, street lighting and signage, connecting three residential areas to the main Ibra–Sur highway.",
            CustomFields = new() {
                new CustomField{Title="Grade requirement", Description="Contractor must hold a valid Grade 1 or Grade 2 civil works classification."}
            }
        },
        new Tender {
            Id="t5", Entity="Ministry of Health",
            Title="Annual Maintenance Contract for HVAC Systems — Royal Hospital",
            TenderNo="128/2026/MOH/ENG-33", ExtId="98042", Category="SERVICES", CategoryLabel="Facilities Management",
            Governorate="Muscat", Wilayat="Bawshar", BudgetLabel="OMR 65K–140K", BudgetMin=65000, BudgetMax=140000,
            Published=new DateOnly(2026,9,1), Closing=new DateOnly(2026,11,10), DocFee="OMR 10.000", Bond="1.5% of quoted value",
            Score=88, IsNew=true, Icon="i-tool",
            Description="Preventive and reactive maintenance of all HVAC systems across hospital wards, operating theatres and administrative buildings under a 12-month renewable contract.",
            CustomFields = new() {
                new CustomField{Title="Response time", Description="Critical faults in operating theatres must be attended within 2 hours, 24/7."}
            }
        },
        new Tender {
            Id="t6", Entity="Ministry of Labour",
            Title="Supply of Laboratory Equipment for Vocational Colleges",
            TenderNo="071/2026/MOL/PROC-05", ExtId="96410", Category="SUPPLY", CategoryLabel="Education & Training",
            Governorate="Al Batinah North", Wilayat="Sohar", BudgetLabel="OMR 140K–260K", BudgetMin=140000, BudgetMax=260000,
            Published=new DateOnly(2026,8,25), Closing=new DateOnly(2026,10,5), DocFee="OMR 20.000", Bond="2.0% of quoted value",
            Score=69, IsNew=false, Icon="i-box",
            Description="Supply, delivery and installation of electronics, mechanical and materials-testing laboratory equipment across four vocational training colleges, with staff training included.",
            CustomFields = new() {
                new CustomField{Title="Delivery schedule", Description="All equipment must be delivered and commissioned before the start of the academic term."}
            }
        }
    };

    public static readonly Dictionary<string, Company> Companies = new()
    {
        ["c1"] = new Company {
            Id="c1", Name="Gulf Systems Integration LLC", Initials="GS", Address="Al Khuwair, Muscat, Oman", Website="gulfsystems.om",
            Sectors = new(){"Technology","Networking","Cloud"}, Overall=4.6, RatingCount=27,
            Bars = new() { ["Quality"]=4.6, ["Communication"]=4.8, ["Experience"]=4.5, ["Deadlines"]=4.3 },
            Bio="Gulf Systems Integration LLC has delivered enterprise networking, cloud and systems-integration projects for government and private-sector clients across Oman since 2011. The team holds Grade 1 IT classification and has completed over 60 public-sector contracts.",
            CustomSections = new() {
                new CustomSection{Tab="Certifications", Title="ISO & vendor certifications", Description="ISO 27001:2022 certified. Cisco Gold Partner, Microsoft Solutions Partner for Infrastructure."},
                new CustomSection{Tab="Portfolio", Title="Selected past work", Description="Network refresh for 40+ government branch offices (2024); national data-centre network redesign (2023); managed SD-WAN rollout for a regional bank (2022)."}
            },
            Reviews = new() {
                new Review{By="Ministry of Transport, Communications and IT", Date=new DateOnly(2026,6,2), Quality=5, Communication=5, Experience=4, Deadline=4, Comment="Delivered the network refresh two days ahead of schedule with clear weekly status updates.", Anonymous=false},
                new Review{By="Anonymous", Date=new DateOnly(2026,4,18), Quality=4, Communication=5, Experience=5, Deadline=4, Comment="Strong technical team. Documentation could have been slightly more detailed at handover.", Anonymous=true}
            }
        },
        ["c2"] = new Company {
            Id="c2", Name="Al Nahda Consulting Group", Initials="AN", Address="Ruwi, Muscat, Oman", Website="alnahdaconsulting.om",
            Sectors = new(){"Consulting","ERP","Change Management"}, Overall=4.3, RatingCount=14,
            Bars = new() { ["Quality"]=4.4, ["Communication"]=4.2, ["Experience"]=4.5, ["Deadlines"]=4.1 },
            Bio="Al Nahda Consulting Group advises public authorities on digital transformation, ERP selection and process re-engineering, with a track record across free-zone and municipal authorities.",
            CustomSections = new() {
                new CustomSection{Tab="Certifications", Title="Accreditations", Description="SAP Certified Partner. PMI-registered project management practice."}
            },
            Reviews = new() {
                new Review{By="Public Authority for Special Economic Zones", Date=new DateOnly(2026,5,10), Quality=4, Communication=4, Experience=5, Deadline=4, Comment="Solid advisory work; the phased rollout plan reduced disruption significantly.", Anonymous=false}
            }
        }
    };

    public static readonly List<TenderRequestRow> Requests = new()
    {
        new(){ TenderId="t1", Company="Gulf Systems Integration LLC", Price=342500, Days=120, Status="ACCEPTED", Date=new DateOnly(2026,9,2) },
        new(){ TenderId="t1", Company="Al Falaj Networks Co.", Price=359900, Days=110, Status="REJECTED", Date=new DateOnly(2026,9,1) },
        new(){ TenderId="t2", Company="Al Nahda Consulting Group", Price=168000, Days=180, Status="PENDING", Date=new DateOnly(2026,9,12) },
        new(){ TenderId="t3", Company="CloudPeak Technologies LLC", Price=610000, Days=90, Status="PENDING", Date=new DateOnly(2026,9,18) },
        new(){ TenderId="t5", Company="Bawshar Technical Services", Price=98500, Days=365, Status="PENDING", Date=new DateOnly(2026,9,20) },
        new(){ TenderId="t5", Company="Comfort Air Solutions LLC", Price=112000, Days=365, Status="PENDING", Date=new DateOnly(2026,9,19) },
        new(){ TenderId="t6", Company="Sohar Scientific Supplies", Price=225000, Days=60, Status="WITHDRAWN", Date=new DateOnly(2026,8,30) }
    };

    public static readonly List<CompanyRow> CompanyRows = new()
    {
        new(){ Name="Gulf Systems Integration LLC", Sectors="Technology, Networking", Rating=4.6, Count=27, Status="Verified" },
        new(){ Name="Al Nahda Consulting Group", Sectors="Consulting, ERP", Rating=4.3, Count=14, Status="Verified" },
        new(){ Name="CloudPeak Technologies LLC", Sectors="Cloud, Cybersecurity", Rating=4.1, Count=9, Status="Pending review" },
        new(){ Name="Bawshar Technical Services", Sectors="Facilities, HVAC", Rating=4.4, Count=22, Status="Verified" },
        new(){ Name="Sohar Scientific Supplies", Sectors="Lab Equipment, Supply", Rating=3.9, Count=6, Status="Verified" }
    };

    public static readonly List<UserRow> UserRows = new()
    {
        new(){ Name="Ahmed Al Rawahi", Email="ahmed@gulfsystems.om", Role="Client admin", Company="Gulf Systems Integration LLC", Joined=new DateOnly(2025,2,14) },
        new(){ Name="Layla Al Hinai", Email="layla@alnahda.om", Role="Contractor", Company="Al Nahda Consulting Group", Joined=new DateOnly(2025,5,3) },
        new(){ Name="Yousuf Al Balushi", Email="yousuf@mtcit.gov.om", Role="Client admin", Company="Ministry of Transport, Communications and IT", Joined=new DateOnly(2024,11,20) },
        new(){ Name="Maha Al Kindi", Email="maha@cloudpeak.om", Role="Contractor", Company="CloudPeak Technologies LLC", Joined=new DateOnly(2026,1,9) }
    };

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
