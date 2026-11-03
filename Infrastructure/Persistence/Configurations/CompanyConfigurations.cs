using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable(t => t.HasCheckConstraint("ck_companies_verification_status", "verification_status IN ('PENDING','VERIFIED')"));

        b.HasIndex(e => e.CompanyEmail).IsUnique();
        b.Property(e => e.DefaultLanguage).HasDefaultValue("en");
        b.Property(e => e.AvgCommunication).HasDefaultValue(0d);
        b.Property(e => e.AvgDeadline).HasDefaultValue(0d);
        b.Property(e => e.AvgExperience).HasDefaultValue(0d);
        b.Property(e => e.AvgOverallRating).HasDefaultValue(0d);
        b.Property(e => e.AvgQuality).HasDefaultValue(0d);
        b.Property(e => e.NumberOfRatings).HasDefaultValue(0);
        b.Property(e => e.VerificationStatus).HasDefaultValue(VerificationStatuses.Pending);
        b.HasTimestamps();

        b.HasIndex(e => e.AvgOverallRating);
        b.HasIndex(e => e.DefaultLanguage);
        b.HasIndex(e => e.Sectors).HasMethod("gin");

        b.HasOne(e => e.DefaultLanguageNavigation).WithMany()
            .HasForeignKey(e => e.DefaultLanguage).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CompanyTranslationConfiguration : IEntityTypeConfiguration<CompanyTranslation>
{
    public void Configure(EntityTypeBuilder<CompanyTranslation> b)
    {
        b.HasTimestamps();
        b.HasIndex(e => new { e.CompanyId, e.LanguageCode }).IsUnique();
        b.HasIndex(e => e.LanguageCode);
        b.HasIndex(e => e.CompanyTitle).HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("ix_company_translations_title_trgm");
        b.HasOne(e => e.Company).WithMany(c => c.Translations).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CustomSectionConfiguration : IEntityTypeConfiguration<CustomSection>
{
    public void Configure(EntityTypeBuilder<CustomSection> b)
    {
        b.Property(e => e.SortOrder).HasDefaultValue(0);
        b.HasTimestamps();
        b.HasIndex(e => new { e.CompanyProfileId, e.SortOrder });
        b.HasOne(e => e.Company).WithMany(c => c.CustomSections).HasForeignKey(e => e.CompanyProfileId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomSectionTranslationConfiguration : IEntityTypeConfiguration<CustomSectionTranslation>
{
    public void Configure(EntityTypeBuilder<CustomSectionTranslation> b)
    {
        b.HasIndex(e => new { e.CustomSectionId, e.LanguageCode }).IsUnique();
        b.HasIndex(e => e.LanguageCode);
        b.HasOne(e => e.CustomSection).WithMany(s => s.Translations).HasForeignKey(e => e.CustomSectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> b)
    {
        b.HasIndex(e => e.Email).IsUnique();
        b.HasIndex(e => e.ClerkUserId).IsUnique();
        b.Property(e => e.PreferredLanguage).HasDefaultValue("en");
        b.Property(e => e.Timezone).HasDefaultValue("Asia/Muscat");
        b.HasTimestamps();

        b.HasIndex(e => e.CompanyId);
        b.HasIndex(e => e.PreferredLanguage);

        b.HasOne(e => e.Company).WithMany(c => c.Users).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.PreferredLanguageNavigation).WithMany()
            .HasForeignKey(e => e.PreferredLanguage).OnDelete(DeleteBehavior.Restrict);
    }
}
