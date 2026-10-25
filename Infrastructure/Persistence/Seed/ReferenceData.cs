namespace Munaqasat.Web.Infrastructure.Persistence.Seed;

public sealed record SectorSeed(string Code, string NameEn, string NameAr);

public sealed record GovernorateSeed(string Code, string NameEn, string NameAr, WilayatSeed[] Wilayats);

public sealed record WilayatSeed(string NameEn, string NameAr);

public sealed record PlanSeed(
    string Code,
    string Name,
    string NameAr,
    string Description,
    decimal? MonthlyPrice,
    string PriceLabel,
    int? MonthlyBidLimit,
    bool IsHighlighted,
    int SortOrder,
    string[] Features);

public static class ReferenceData
{
    public static readonly SectorSeed[] Sectors =
    [
        new("TECHNOLOGY", "Technology", "تقنية المعلومات"),
        new("NETWORKING", "Networking", "الشبكات"),
        new("CLOUD", "Cloud", "الحوسبة السحابية"),
        new("CYBERSECURITY", "Cybersecurity", "الأمن السيبراني"),
        new("CONSULTING", "Consulting", "الاستشارات"),
        new("ERP", "ERP", "تخطيط موارد المؤسسات"),
        new("ROADS", "Roads & Infrastructure", "الطرق والبنية الأساسية"),
        new("CONSTRUCTION", "Construction", "الإنشاءات"),
        new("FACILITIES", "Facilities Management", "إدارة المرافق"),
        new("HVAC", "HVAC", "التكييف والتهوية"),
        new("EDUCATION", "Education & Training", "التعليم والتدريب"),
        new("HEALTHCARE", "Healthcare", "الرعاية الصحية"),
        new("LAB_EQUIPMENT", "Laboratory Equipment", "الأجهزة المختبرية"),
        new("SUPPLY_CHAIN", "Supply Chain", "سلاسل الإمداد"),
        new("ENERGY", "Energy", "الطاقة"),
        new("WATER", "Water & Environment", "المياه والبيئة")
    ];

    public static readonly GovernorateSeed[] Governorates =
    [
        new("MUSCAT", "Muscat", "مسقط",
        [
            new("Muscat", "مسقط"),
            new("Muttrah", "مطرح"),
            new("Bawshar", "بوشر"),
            new("Seeb", "السيب"),
            new("Al Amerat", "العامرات"),
            new("Qurayyat", "قريات")
        ]),
        new("DHOFAR", "Dhofar", "ظفار",
        [
            new("Salalah", "صلالة"),
            new("Taqah", "طاقة"),
            new("Mirbat", "مرباط"),
            new("Thumrait", "ثمريت"),
            new("Sadah", "سدح"),
            new("Rakhyut", "رخيوت"),
            new("Dhalkut", "ضلكوت"),
            new("Al Mazyona", "المزيونة"),
            new("Muqshin", "مقشن"),
            new("Shalim and the Hallaniyat Islands", "شليم وجزر الحلانيات")
        ]),
        new("MUSANDAM", "Musandam", "مسندم",
        [
            new("Khasab", "خصب"),
            new("Bukha", "بخا"),
            new("Daba", "دبا"),
            new("Madha", "مدحاء")
        ]),
        new("AL_BURAIMI", "Al Buraimi", "البريمي",
        [
            new("Al Buraimi", "البريمي"),
            new("Mahdah", "محضة"),
            new("As Sunaynah", "السنينة")
        ]),
        new("AL_DAKHILIYAH", "Al Dakhiliyah", "الداخلية",
        [
            new("Nizwa", "نزوى"),
            new("Bahla", "بهلاء"),
            new("Manah", "منح"),
            new("Al Hamra", "الحمراء"),
            new("Adam", "أدم"),
            new("Izki", "إزكي"),
            new("Samail", "سمائل"),
            new("Bidbid", "بدبد")
        ]),
        new("AL_BATINAH_NORTH", "Al Batinah North", "شمال الباطنة",
        [
            new("Sohar", "صحار"),
            new("Shinas", "شناص"),
            new("Liwa", "لوى"),
            new("Saham", "صحم"),
            new("Al Khaburah", "الخابورة"),
            new("As Suwaiq", "السويق")
        ]),
        new("AL_BATINAH_SOUTH", "Al Batinah South", "جنوب الباطنة",
        [
            new("Ar Rustaq", "الرستاق"),
            new("Al Awabi", "العوابي"),
            new("Nakhal", "نخل"),
            new("Wadi Al Maawil", "وادي المعاول"),
            new("Barka", "بركاء"),
            new("Al Musanaah", "المصنعة")
        ]),
        new("AL_SHARQIYAH_NORTH", "Al Sharqiyah North", "شمال الشرقية",
        [
            new("Ibra", "إبراء"),
            new("Al Mudhaibi", "المضيبي"),
            new("Bidiyah", "بدية"),
            new("Wadi Bani Khalid", "وادي بني خالد"),
            new("Dima Wa At Taeyeen", "دماء والطائيين"),
            new("Al Qabil", "القابل")
        ]),
        new("AL_SHARQIYAH_SOUTH", "Al Sharqiyah South", "جنوب الشرقية",
        [
            new("Sur", "صور"),
            new("Al Kamil Wal Wafi", "الكامل والوافي"),
            new("Jalan Bani Bu Hassan", "جعلان بني بو حسن"),
            new("Jalan Bani Bu Ali", "جعلان بني بو علي"),
            new("Masirah", "مصيرة")
        ]),
        new("AL_DHAHIRAH", "Al Dhahirah", "الظاهرة",
        [
            new("Ibri", "عبري"),
            new("Yanqul", "ينقل"),
            new("Dhank", "ضنك")
        ]),
        new("AL_WUSTA", "Al Wusta", "الوسطى",
        [
            new("Haima", "هيما"),
            new("Mahout", "محوت"),
            new("Duqm", "الدقم"),
            new("Al Jazer", "الجازر")
        ])
    ];

    public static readonly PlanSeed[] Plans =
    [
        new(
            Code: "STARTER",
            Name: "Starter",
            NameAr: "المبتدئ",
            Description: "For occasional bidders trying the platform",
            MonthlyPrice: 0m,
            PriceLabel: "Free",
            MonthlyBidLimit: 1,
            IsHighlighted: false,
            SortOrder: 0,
            Features:
            [
                "Browse all open tenders",
                "1 active bid request",
                "Basic company profile",
                "Email support"
            ]),
        new(
            Code: "PROFESSIONAL",
            Name: "Professional",
            NameAr: "المحترف",
            Description: "For contractors actively bidding every month",
            MonthlyPrice: 29m,
            PriceLabel: "OMR 29 / month",
            MonthlyBidLimit: null,
            IsHighlighted: true,
            SortOrder: 1,
            Features:
            [
                "Everything in Starter",
                "Unlimited bid requests",
                "Match score and smart alerts",
                "Verified profile badge",
                "Priority support"
            ]),
        new(
            Code: "ENTERPRISE",
            Name: "Enterprise",
            NameAr: "المؤسسات",
            Description: "For government entities and large issuers",
            MonthlyPrice: null,
            PriceLabel: "Custom",
            MonthlyBidLimit: null,
            IsHighlighted: false,
            SortOrder: 2,
            Features:
            [
                "Everything in Professional",
                "Unlimited tender postings",
                "Team seats and approvals",
                "Dedicated account manager",
                "SLA and onboarding support"
            ])
    ];
}
