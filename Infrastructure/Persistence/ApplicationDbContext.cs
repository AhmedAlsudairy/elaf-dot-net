using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services;

namespace Munaqasat.Web.Infrastructure.Persistence;

/// <summary>
/// Tables follow the tender platform ERD (snake_case, text ids, one translation row per record and language).
/// ASP.NET Core Identity keeps its own tables for passwords and roles; <c>user_profiles.identity_user_id</c>
/// links a profile to its Identity user.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    private readonly IClock _clock;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock)
        : base(options) => _clock = clock;

    public DbSet<Language> Languages => Set<Language>();
    public DbSet<UiTranslation> UiTranslations => Set<UiTranslation>();
    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<TenderTranslation> TenderTranslations => Set<TenderTranslation>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldTranslation> CustomFieldTranslations => Set<CustomFieldTranslation>();
    public DbSet<TenderRequest> TenderRequests => Set<TenderRequest>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyTranslation> CompanyTranslations => Set<CompanyTranslation>();
    public DbSet<CustomSection> CustomSections => Set<CustomSection>();
    public DbSet<CustomSectionTranslation> CustomSectionTranslations => Set<CustomSectionTranslation>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<CompanyRating> CompanyRatings => Set<CompanyRating>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresEnum<TextDirection>(name: "text_direction");
        builder.HasPostgresEnum<RequestStatus>(name: "request_status");
        builder.HasPostgresExtension("pg_trgm");

        // Identity tables get a clear prefix so they never collide with the ERD tables.
        builder.Entity<IdentityUser>().ToTable("identity_users");
        builder.Entity<IdentityRole>().ToTable("identity_roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("identity_user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("identity_user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("identity_user_logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("identity_user_tokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("identity_role_claims");

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampTimestamps();
        return base.SaveChanges();
    }

    private void StampTimestamps()
    {
        var now = _clock.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IHasCreatedAt>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                if (entry.Entity is IHasTimestamps added) added.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.CreatedAt).IsModified = false;
                if (entry.Entity is IHasTimestamps modified) modified.UpdatedAt = now;
            }
        }
    }
}
