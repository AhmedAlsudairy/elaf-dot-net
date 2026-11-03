using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> b)
    {
        b.HasTimestamps();
        b.HasIndex(e => e.TenderId);
        b.HasIndex(e => e.TenderRequestId);
        b.HasIndex(e => new { e.SenderCompanyProfileId, e.ReceiverCompanyProfileId });
        b.HasIndex(e => e.ReceiverCompanyProfileId);

        b.HasOne(e => e.Tender).WithMany().HasForeignKey(e => e.TenderId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.TenderRequest).WithMany().HasForeignKey(e => e.TenderRequestId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.SenderCompany).WithMany().HasForeignKey(e => e.SenderCompanyProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.ReceiverCompany).WithMany().HasForeignKey(e => e.ReceiverCompanyProfileId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.Property(e => e.ReadStatus).HasDefaultValue(false);
        b.HasTimestamps();
        b.HasIndex(e => new { e.ChatRoomId, e.CreatedAt });
        b.HasIndex(e => e.SenderCompanyProfileId);
        b.HasIndex(e => e.ReceiverCompanyProfileId);
        b.HasIndex(e => e.LanguageCode);

        b.HasOne(e => e.ChatRoom).WithMany(r => r.Messages).HasForeignKey(e => e.ChatRoomId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.SenderCompany).WithMany().HasForeignKey(e => e.SenderCompanyProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.ReceiverCompany).WithMany().HasForeignKey(e => e.ReceiverCompanyProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CompanyRatingConfiguration : IEntityTypeConfiguration<CompanyRating>
{
    public void Configure(EntityTypeBuilder<CompanyRating> b)
    {
        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_company_ratings_quality", "quality BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_company_ratings_communication", "communication BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_company_ratings_experience", "experience BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_company_ratings_deadline", "deadline BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_company_ratings_overall", "overall_rating BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_company_ratings_not_self", "rating_company_id <> rated_company_id");
        });

        b.Property(e => e.Anonymous).HasDefaultValue(false);
        b.HasCreatedAt();

        b.HasIndex(e => new { e.RatingCompanyId, e.TenderRequestId }).IsUnique();
        b.HasIndex(e => e.RatedCompanyId);
        b.HasIndex(e => e.TenderId);
        b.HasIndex(e => e.TenderRequestId);
        b.HasIndex(e => e.LanguageCode);

        b.HasOne(e => e.RatingCompany).WithMany(c => c.RatingsGiven).HasForeignKey(e => e.RatingCompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.RatedCompany).WithMany(c => c.RatingsReceived).HasForeignKey(e => e.RatedCompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Tender).WithMany().HasForeignKey(e => e.TenderId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.TenderRequest).WithMany().HasForeignKey(e => e.TenderRequestId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageCode).OnDelete(DeleteBehavior.SetNull);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.Property(e => e.Read).HasDefaultValue(false);
        b.HasCreatedAt();

        b.HasIndex(e => new { e.UserId, e.CreatedAt });
        b.HasIndex(e => new { e.UserId, e.CreatedAt }, "ix_notifications_unread").HasFilter("NOT read");
        b.HasIndex(e => e.TenderId);
        b.HasIndex(e => e.ActorId);

        b.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Actor).WithMany().HasForeignKey(e => e.ActorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.Tender).WithMany().HasForeignKey(e => e.TenderId).OnDelete(DeleteBehavior.Cascade);
    }
}
