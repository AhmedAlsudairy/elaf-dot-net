using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("Bids");

        builder.Property(b => b.Price).HasPrecision(18, 3);
        builder.Property(b => b.Currency).HasMaxLength(3).IsRequired();
        builder.Property(b => b.Notes).HasMaxLength(4000);
        builder.Property(b => b.DecisionNote).HasMaxLength(2000);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(b => b.IsLive);

        builder.HasOne(b => b.Tender)
            .WithMany(t => t.Bids)
            .HasForeignKey(b => b.TenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Company)
            .WithMany(c => c.Bids)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.SubmittedByUser)
            .WithMany(u => u.SubmittedBids)
            .HasForeignKey(b => b.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.TenderId, b.Status });
        builder.HasIndex(b => new { b.CompanyId, b.SubmittedAt });
    }
}
