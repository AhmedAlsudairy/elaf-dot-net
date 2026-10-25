using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.Property(c => c.Name).HasMaxLength(250).IsRequired();
        builder.Property(c => c.NameAr).HasMaxLength(250);
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Initials).HasMaxLength(4);
        builder.Property(c => c.CommercialRegistrationNumber).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(400);
        builder.Property(c => c.Website).HasMaxLength(250);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        builder.Property(c => c.VerificationStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasIndex(c => c.CommercialRegistrationNumber)
            .IsUnique()
            .HasFilter("\"CommercialRegistrationNumber\" IS NOT NULL");

        builder.HasIndex(c => c.VerificationStatus);

        builder.HasOne(c => c.Governorate)
            .WithMany()
            .HasForeignKey(c => c.GovernorateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.ProfileSections)
            .WithOne(s => s.Company)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CompanySectorConfiguration : IEntityTypeConfiguration<CompanySector>
{
    public void Configure(EntityTypeBuilder<CompanySector> builder)
    {
        builder.ToTable("CompanySectors");

        builder.HasKey(cs => new { cs.CompanyId, cs.SectorId });

        builder.HasOne(cs => cs.Company)
            .WithMany(c => c.Sectors)
            .HasForeignKey(cs => cs.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Sector)
            .WithMany(s => s.Companies)
            .HasForeignKey(cs => cs.SectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cs => cs.SectorId);
    }
}

public class CompanyProfileSectionConfiguration : IEntityTypeConfiguration<CompanyProfileSection>
{
    public void Configure(EntityTypeBuilder<CompanyProfileSection> builder)
    {
        builder.ToTable("CompanyProfileSections");

        builder.Property(s => s.Tab).HasMaxLength(80).IsRequired();
        builder.Property(s => s.Title).HasMaxLength(250).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(4000);

        builder.HasIndex(s => new { s.CompanyId, s.SortOrder });
    }
}
