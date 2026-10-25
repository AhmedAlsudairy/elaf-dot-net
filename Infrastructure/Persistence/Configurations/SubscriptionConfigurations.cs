using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Property(p => p.NameAr).HasMaxLength(120);
        builder.Property(p => p.Description).HasMaxLength(600);
        builder.Property(p => p.PriceLabel).HasMaxLength(60);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.MonthlyPrice).HasPrecision(18, 3);

        builder.HasIndex(p => p.Code).IsUnique();

        builder.HasMany(p => p.Features)
            .WithOne(f => f.SubscriptionPlan)
            .HasForeignKey(f => f.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SubscriptionPlanFeatureConfiguration : IEntityTypeConfiguration<SubscriptionPlanFeature>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanFeature> builder)
    {
        builder.ToTable("SubscriptionPlanFeatures");

        builder.Property(f => f.Label).HasMaxLength(250).IsRequired();
        builder.Property(f => f.LabelAr).HasMaxLength(250);

        builder.HasIndex(f => new { f.SubscriptionPlanId, f.SortOrder });
    }
}

public class CompanySubscriptionConfiguration : IEntityTypeConfiguration<CompanySubscription>
{
    public void Configure(EntityTypeBuilder<CompanySubscription> builder)
    {
        builder.ToTable("CompanySubscriptions");

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(s => s.IsCurrent);

        builder.HasOne(s => s.Company)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SubscriptionPlan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.CompanyId, s.Status });
    }
}
