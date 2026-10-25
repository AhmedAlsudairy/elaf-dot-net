using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectors");

        builder.Property(s => s.Code).HasMaxLength(50).IsRequired();
        builder.Property(s => s.NameEn).HasMaxLength(120).IsRequired();
        builder.Property(s => s.NameAr).HasMaxLength(120).IsRequired();

        builder.HasIndex(s => s.Code).IsUnique();
    }
}

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");

        builder.Property(g => g.Code).HasMaxLength(50).IsRequired();
        builder.Property(g => g.NameEn).HasMaxLength(120).IsRequired();
        builder.Property(g => g.NameAr).HasMaxLength(120).IsRequired();

        builder.HasIndex(g => g.Code).IsUnique();

        builder.HasMany(g => g.Wilayats)
            .WithOne(w => w.Governorate)
            .HasForeignKey(w => w.GovernorateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WilayatConfiguration : IEntityTypeConfiguration<Wilayat>
{
    public void Configure(EntityTypeBuilder<Wilayat> builder)
    {
        builder.ToTable("Wilayats");

        builder.Property(w => w.NameEn).HasMaxLength(120).IsRequired();
        builder.Property(w => w.NameAr).HasMaxLength(120).IsRequired();

        builder.HasIndex(w => new { w.GovernorateId, w.NameEn }).IsUnique();
    }
}
