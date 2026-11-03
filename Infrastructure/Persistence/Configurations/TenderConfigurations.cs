using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using static Munaqasat.Web.Infrastructure.Persistence.Configurations.ModelBuilderExtensions;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> b)
    {
        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_tenders_bond_percentage", "bond_percentage IS NULL OR bond_percentage BETWEEN 0 AND 100");
            t.HasCheckConstraint("ck_tenders_document_fee", "document_fee IS NULL OR document_fee >= 0");
            t.HasCheckConstraint("ck_tenders_budget", "budget_min IS NULL OR budget_max IS NULL OR budget_min <= budget_max");
            t.HasCheckConstraint("ck_tenders_category", "procurement_category_code IN ('SUPPLY','SERVICES','WORKS')");
            t.HasCheckConstraint("ck_tenders_status", "status IN ('DRAFT','PUBLISHED')");
        });

        b.HasIndex(e => e.ExternalTenderId).IsUnique();
        b.Property(e => e.IsAlternateBidAllowed).HasDefaultValue(false);
        b.Property(e => e.IsEBondApplicable).HasDefaultValue(false);
        b.Property(e => e.DocumentFee).HasColumnType(Money);
        b.Property(e => e.BondPercentage).HasColumnType("numeric(6,3)");
        b.Property(e => e.BudgetMin).HasColumnType(Money);
        b.Property(e => e.BudgetMax).HasColumnType(Money);
        b.Property(e => e.OriginalLanguage).HasDefaultValue("en");
        b.Property(e => e.Status).HasDefaultValue(TenderStatuses.Published);
        b.HasTimestamps();

        b.HasIndex(e => e.BidSubmissionClosing);
        b.HasIndex(e => e.PublishedDate);
        b.HasIndex(e => e.ProcurementCategoryCode);
        b.HasIndex(e => e.OriginalLanguage);
        b.HasIndex(e => e.VendorGrades).HasMethod("gin");

        b.HasOne(e => e.OriginalLanguageNavigation).WithMany()
            .HasForeignKey(e => e.OriginalLanguage).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TenderTranslationConfiguration : IEntityTypeConfiguration<TenderTranslation>
{
    public void Configure(EntityTypeBuilder<TenderTranslation> b)
    {
        b.Property(e => e.IsMachineTranslated).HasDefaultValue(false);
        b.HasTimestamps();

        b.HasIndex(e => new { e.TenderId, e.LanguageCode }).IsUnique();
        b.HasIndex(e => e.LanguageCode);
        b.HasIndex(e => new { e.Governorate, e.Wilayat });
        b.HasIndex(e => e.Title).HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("ix_tender_translations_title_trgm");

        b.HasOne(e => e.Tender).WithMany(t => t.Translations).HasForeignKey(e => e.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CustomFieldConfiguration : IEntityTypeConfiguration<CustomField>
{
    public void Configure(EntityTypeBuilder<CustomField> b)
    {
        b.Property(e => e.SortOrder).HasDefaultValue(0);
        b.HasTimestamps();
        b.HasIndex(e => new { e.TenderId, e.SortOrder });
        b.HasOne(e => e.Tender).WithMany(t => t.CustomFields).HasForeignKey(e => e.TenderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomFieldTranslationConfiguration : IEntityTypeConfiguration<CustomFieldTranslation>
{
    public void Configure(EntityTypeBuilder<CustomFieldTranslation> b)
    {
        b.HasIndex(e => new { e.CustomFieldId, e.LanguageCode }).IsUnique();
        b.HasIndex(e => e.LanguageCode);
        b.HasOne(e => e.CustomField).WithMany(f => f.Translations).HasForeignKey(e => e.CustomFieldId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TenderRequestConfiguration : IEntityTypeConfiguration<TenderRequest>
{
    public void Configure(EntityTypeBuilder<TenderRequest> b)
    {
        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_tender_requests_price", "price IS NULL OR price >= 0");
            t.HasCheckConstraint("ck_tender_requests_days", "days IS NULL OR days > 0");
        });

        b.Property(e => e.Status).HasDefaultValue(RequestStatus.Pending);
        b.Property(e => e.Price).HasColumnType(Money);
        b.HasTimestamps();

        b.HasIndex(e => new { e.TenderId, e.CompanyProfileId }).IsUnique();
        b.HasIndex(e => new { e.CompanyProfileId, e.Status });
        b.HasIndex(e => new { e.TenderId, e.Status });
        b.HasIndex(e => e.LanguageCode);

        b.HasOne(e => e.Tender).WithMany(t => t.Requests).HasForeignKey(e => e.TenderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Company).WithMany(c => c.Requests).HasForeignKey(e => e.CompanyProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.SetNull);
    }
}
