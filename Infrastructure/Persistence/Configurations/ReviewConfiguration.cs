using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", t =>
        {
            t.HasCheckConstraint("CK_Reviews_QualityRating", "\"QualityRating\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_CommunicationRating", "\"CommunicationRating\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_ExperienceRating", "\"ExperienceRating\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("CK_Reviews_DeadlineRating", "\"DeadlineRating\" BETWEEN 1 AND 5");
        });

        builder.Property(r => r.Comment).HasMaxLength(4000);

        builder.Ignore(r => r.Overall);

        builder.HasOne(r => r.Company)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.AuthorUser)
            .WithMany(u => u.AuthoredReviews)
            .HasForeignKey(r => r.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Tender)
            .WithMany()
            .HasForeignKey(r => r.TenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.CompanyId, r.AuthorUserId }).IsUnique();

        builder.HasIndex(r => new { r.CompanyId, r.IsPublished });
    }
}
