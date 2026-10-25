using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> builder)
    {
        builder.ToTable("Tenders");

        builder.Property(t => t.Reference).HasMaxLength(120).IsRequired();
        builder.Property(t => t.ExternalId).HasMaxLength(120);
        builder.Property(t => t.Title).HasMaxLength(400).IsRequired();
        builder.Property(t => t.TitleAr).HasMaxLength(400);
        builder.Property(t => t.Description).HasMaxLength(8000);
        builder.Property(t => t.DescriptionAr).HasMaxLength(8000);
        builder.Property(t => t.IssuingEntity).HasMaxLength(300).IsRequired();
        builder.Property(t => t.CategoryLabel).HasMaxLength(120);
        builder.Property(t => t.Currency).HasMaxLength(3).IsRequired();
        builder.Property(t => t.Icon).HasMaxLength(40);

        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(t => t.BudgetMin).HasPrecision(18, 3);
        builder.Property(t => t.BudgetMax).HasPrecision(18, 3);
        builder.Property(t => t.DocumentFee).HasPrecision(18, 3);
        builder.Property(t => t.BidBondPercentage).HasPrecision(5, 2);

        builder.HasIndex(t => t.ExternalId);

        builder.HasIndex(t => new { t.Status, t.ClosingOn });
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.GovernorateId);
        builder.HasIndex(t => t.OwnerCompanyId);

        builder.HasOne(t => t.Governorate)
            .WithMany()
            .HasForeignKey(t => t.GovernorateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Wilayat)
            .WithMany()
            .HasForeignKey(t => t.WilayatId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.OwnerCompany)
            .WithMany()
            .HasForeignKey(t => t.OwnerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.CustomFields)
            .WithOne(f => f.Tender)
            .HasForeignKey(f => f.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Tender)
            .HasForeignKey(a => a.TenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TenderCustomFieldConfiguration : IEntityTypeConfiguration<TenderCustomField>
{
    public void Configure(EntityTypeBuilder<TenderCustomField> builder)
    {
        builder.ToTable("TenderCustomFields");

        builder.Property(f => f.Title).HasMaxLength(250).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(4000);

        builder.HasIndex(f => new { f.TenderId, f.SortOrder });
    }
}

public class TenderAttachmentConfiguration : IEntityTypeConfiguration<TenderAttachment>
{
    public void Configure(EntityTypeBuilder<TenderAttachment> builder)
    {
        builder.ToTable("TenderAttachments");

        builder.Property(a => a.FileName).HasMaxLength(300).IsRequired();
        builder.Property(a => a.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(150).IsRequired();

        builder.HasIndex(a => a.TenderId);
    }
}

public class TenderMatchConfiguration : IEntityTypeConfiguration<TenderMatch>
{
    public void Configure(EntityTypeBuilder<TenderMatch> builder)
    {
        builder.ToTable("TenderMatches");

        builder.HasKey(m => new { m.TenderId, m.CompanyId });

        builder.Property(m => m.Reason).HasMaxLength(300);

        builder.HasOne(m => m.Tender)
            .WithMany(t => t.Matches)
            .HasForeignKey(m => m.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Company)
            .WithMany()
            .HasForeignKey(m => m.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.CompanyId, m.Score });
    }
}
