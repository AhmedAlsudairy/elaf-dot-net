using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Seed;

/// <summary>
/// Arabic translations of the sample tenders and companies (Development only, with <c>Seed:SampleData</c>).
/// Adds only the Arabic rows that are missing, so it is safe to run on every startup and fills databases
/// seeded before these translations existed.
/// </summary>
internal static class ArabicSampleContent
{
    private record TenderAr(string Entity, string Title, string Category, string SubCategory, string Governorate,
        string Wilayat, string Note, (string Title, string Description)[] Fields);

    private record CompanyAr(string Name, string Address, string? Bio = null,
        (string Tab, string Title, string Description)[]? Sections = null);

    private static readonly Dictionary<string, TenderAr> Tenders = new()
    {
        ["t1"] = new("وزارة النقل والاتصالات وتقنية المعلومات", "توريد وتركيب معدات البنية التحتية للشبكات",
            "توريد", "التقنية", "مسقط", "بوشر",
            "شراء وتوريد وتركيب معدات الشبكات الأساسية وطبقة الوصول في ثلاثة مراكز بيانات إقليمية، بما في ذلك عقد صيانة في الموقع لمدة 3 سنوات وتدريب الموظفين على الاستلام.",
            new[] { ("فترة الضمان", "ضمان من الشركة المصنعة في الموقع لمدة 3 سنوات على الأقل لجميع الأجهزة الموردة."),
                    ("المحتوى المحلي", "يُشترط ألا تقل نسبة التعمين بين الكوادر الفنية المكلفة عن 35%.") }),
        ["t2"] = new("الهيئة العامة للمناطق الاقتصادية الخاصة والمناطق الحرة", "استشارات التحول الرقمي وتطبيق نظام تخطيط موارد المؤسسة",
            "خدمات", "الاستشارات", "الوسطى", "الدقم",
            "مهمة استشارية لتقييم جاهزية نظام تخطيط موارد المؤسسة الحالي وقيادة تطبيقه على مراحل في وحدات المالية والمشتريات والموارد البشرية لإدارة المنطقة الاقتصادية الخاصة بالدقم.",
            new[] { ("تشكيل الفريق", "يجب أن يحدد العرض مستشارًا رئيسيًا لا تقل خبرته في تطبيق أنظمة تخطيط موارد المؤسسة عن 8 سنوات.") }),
        ["t3"] = new("وزارة المالية", "خدمات الترحيل إلى السحابة والاستضافة المُدارة لمركز البيانات الحكومي",
            "خدمات", "الحوسبة السحابية", "مسقط", "السيب",
            "ترحيل أحمال العمل المحلية القديمة إلى بيئة سحابية هجينة مع استضافة مُدارة ونسخ احتياطي ومراقبة على مدار الساعة بموجب اتفاقية خدمة لمدة 3 سنوات.",
            new[] { ("إقامة البيانات", "يجب أن تبقى جميع بيانات المواطنين مستضافة داخل أراضي سلطنة عُمان."),
                    ("اتفاقية مستوى الخدمة", "توفر شهري بنسبة 99.9% مع غرامات مالية عند الإخلال.") }),
        ["t4"] = new("وزارة النقل", "إنشاء طرق فرعية — ولاية إبراء، المرحلة الثانية",
            "أشغال", "الطرق والبنية الأساسية", "شمال الشرقية", "إبراء",
            "إنشاء 14 كم من الطرق الفرعية المسفلتة شاملة أعمال تصريف المياه والإنارة واللوحات الإرشادية، لربط ثلاث مناطق سكنية بطريق إبراء–صور الرئيسي.",
            new[] { ("الدرجة المطلوبة", "يجب أن يحمل المقاول تصنيفًا ساريًا للأعمال المدنية من الدرجة الأولى أو الثانية.") }),
        ["t5"] = new("وزارة الصحة", "عقد صيانة سنوي لأنظمة التكييف والتهوية — المستشفى السلطاني",
            "خدمات", "إدارة المرافق", "مسقط", "بوشر",
            "صيانة وقائية وتصحيحية لجميع أنظمة التكييف والتهوية في أجنحة المستشفى وغرف العمليات والمباني الإدارية بموجب عقد مدته 12 شهرًا قابل للتجديد.",
            new[] { ("زمن الاستجابة", "يجب الاستجابة للأعطال الحرجة في غرف العمليات خلال ساعتين، على مدار الساعة طوال أيام الأسبوع.") }),
        ["t6"] = new("وزارة العمل", "توريد معدات مختبرات للكليات المهنية",
            "توريد", "التعليم والتدريب", "شمال الباطنة", "صحار",
            "توريد وتسليم وتركيب معدات مختبرات الإلكترونيات والميكانيكا واختبار المواد في أربع كليات للتدريب المهني، مع تدريب الموظفين.",
            new[] { ("جدول التسليم", "يجب تسليم جميع المعدات وتشغيلها قبل بداية الفصل الدراسي.") })
    };

    private static readonly Dictionary<string, CompanyAr> Companies = new()
    {
        ["c1"] = new("الخليج لتكامل الأنظمة ش.م.م", "الخوير، مسقط، عُمان",
            "تنفذ الخليج لتكامل الأنظمة مشاريع الشبكات المؤسسية والحوسبة السحابية وتكامل الأنظمة للجهات الحكومية والخاصة في عُمان منذ عام 2011. يحمل الفريق تصنيف الدرجة الأولى في تقنية المعلومات وأنجز أكثر من 60 عقدًا للقطاع العام.",
            new[] { ("الشهادات", "شهادات الأيزو والموردين", "حاصلة على شهادة ISO 27001:2022. شريك ذهبي لسيسكو، وشريك حلول مايكروسوفت للبنية التحتية."),
                    ("الأعمال", "أعمال سابقة مختارة", "تحديث شبكات أكثر من 40 فرعًا حكوميًا (2024)؛ إعادة تصميم شبكة مركز البيانات الوطني (2023)؛ تطبيق شبكة SD-WAN مُدارة لبنك إقليمي (2022).") }),
        ["c2"] = new("مجموعة النهضة للاستشارات", "روي، مسقط، عُمان",
            "تقدم مجموعة النهضة للاستشارات المشورة للهيئات العامة في التحول الرقمي واختيار أنظمة تخطيط موارد المؤسسة وإعادة هندسة الإجراءات، ولها سجل حافل مع هيئات المناطق الحرة والبلديات.",
            new[] { ("الشهادات", "الاعتمادات", "شريك معتمد من SAP. ممارسة إدارة مشاريع مسجلة لدى معهد إدارة المشاريع (PMI).") }),
        ["c3"] = new("كلاود بيك للتقنيات ش.م.م", "مسقط، عُمان"),
        ["c4"] = new("بوشر للخدمات الفنية", "بوشر، مسقط، عُمان"),
        ["c5"] = new("صحار للتجهيزات العلمية", "صحار، شمال الباطنة، عُمان"),
        ["c6"] = new("شبكات الفلج", "مسقط، عُمان"),
        ["c7"] = new("حلول الراحة للتكييف ش.م.م", "مسقط، عُمان"),
        ["g1"] = new("وزارة النقل والاتصالات وتقنية المعلومات", "مسقط، عُمان"),
        ["g2"] = new("الهيئة العامة للمناطق الاقتصادية الخاصة والمناطق الحرة", "الدقم، الوسطى، عُمان"),
        ["g3"] = new("وزارة المالية", "مسقط، عُمان")
    };

    public static async Task SeedAsync(ApplicationDbContext db, ILogger log)
    {
        var added = 0;

        var tenders = await db.Tenders
            .Include(t => t.Translations)
            .Include(t => t.CustomFields).ThenInclude(f => f.Translations)
            .Where(t => Tenders.Keys.Contains(t.Id))
            .ToListAsync();

        foreach (var tender in tenders)
        {
            var ar = Tenders[tender.Id];
            if (!tender.Translations.Any(t => t.LanguageCode == "ar"))
            {
                tender.Translations.Add(new TenderTranslation
                {
                    LanguageCode = "ar", EntityName = ar.Entity, Title = ar.Title, ProcurementCategory = ar.Category,
                    ProcurementSubCategory = ar.SubCategory, Governorate = ar.Governorate, Wilayat = ar.Wilayat, Note = ar.Note
                });
                added++;
            }

            var fields = tender.CustomFields.OrderBy(f => f.SortOrder).ToList();
            for (var i = 0; i < fields.Count && i < ar.Fields.Length; i++)
            {
                if (fields[i].Translations.Any(t => t.LanguageCode == "ar")) continue;
                fields[i].Translations.Add(new CustomFieldTranslation
                {
                    LanguageCode = "ar", Title = ar.Fields[i].Title, Description = ar.Fields[i].Description
                });
                added++;
            }
        }

        var companies = await db.Companies
            .Include(c => c.Translations)
            .Include(c => c.CustomSections).ThenInclude(s => s.Translations)
            .Where(c => Companies.Keys.Contains(c.Id))
            .ToListAsync();

        foreach (var company in companies)
        {
            var ar = Companies[company.Id];
            if (!company.Translations.Any(t => t.LanguageCode == "ar"))
            {
                company.Translations.Add(new CompanyTranslation
                {
                    LanguageCode = "ar", CompanyTitle = ar.Name, Address = ar.Address, Bio = ar.Bio
                });
                added++;
            }

            var sections = company.CustomSections.OrderBy(s => s.SortOrder).ToList();
            var arSections = ar.Sections ?? Array.Empty<(string, string, string)>();
            for (var i = 0; i < sections.Count && i < arSections.Length; i++)
            {
                if (sections[i].Translations.Any(t => t.LanguageCode == "ar")) continue;
                sections[i].Translations.Add(new CustomSectionTranslation
                {
                    LanguageCode = "ar", TabName = arSections[i].Tab, Title = arSections[i].Title, Description = arSections[i].Description
                });
                added++;
            }
        }

        if (added > 0)
        {
            await db.SaveChangesAsync();
            log.LogInformation("Added {Count} Arabic sample translations", added);
        }
    }
}
