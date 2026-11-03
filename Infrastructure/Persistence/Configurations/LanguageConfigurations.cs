using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> b)
    {
        b.HasKey(e => e.Code);
        b.Property(e => e.Direction).HasDefaultValue(TextDirection.Ltr);
        b.Property(e => e.IsActive).HasDefaultValue(true);
        b.Property(e => e.IsDefault).HasDefaultValue(false);
        b.Property(e => e.SortOrder).HasDefaultValue(0);

        // Only one language may be the default.
        b.HasIndex(e => e.IsDefault).IsUnique().HasFilter("is_default").HasDatabaseName("ux_languages_single_default");
    }
}

public class UiTranslationConfiguration : IEntityTypeConfiguration<UiTranslation>
{
    public void Configure(EntityTypeBuilder<UiTranslation> b)
    {
        b.HasTimestamps();
        b.HasIndex(e => new { e.Namespace, e.Key, e.LanguageCode }).IsUnique();
        b.HasIndex(e => e.LanguageCode);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}
