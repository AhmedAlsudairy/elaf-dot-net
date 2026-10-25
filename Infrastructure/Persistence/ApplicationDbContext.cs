using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services;

namespace Munaqasat.Web.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    private readonly IClock _clock;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock)
        : base(options) => _clock = clock;

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanySector> CompanySectors => Set<CompanySector>();
    public DbSet<CompanyProfileSection> CompanyProfileSections => Set<CompanyProfileSection>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Wilayat> Wilayats => Set<Wilayat>();
    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<TenderCustomField> TenderCustomFields => Set<TenderCustomField>();
    public DbSet<TenderAttachment> TenderAttachments => Set<TenderAttachment>();
    public DbSet<TenderMatch> TenderMatches => Set<TenderMatch>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures => Set<SubscriptionPlanFeature>();
    public DbSet<CompanySubscription> CompanySubscriptions => Set<CompanySubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditColumns();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampAuditColumns();
        return base.SaveChanges();
    }

    private void StampAuditColumns()
    {
        var now = _clock.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}
