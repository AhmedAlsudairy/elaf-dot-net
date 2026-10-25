using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<AppRole> _roles;
    private readonly UserManager<AppUser> _users;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext db,
        RoleManager<AppRole> roles,
        UserManager<AppUser> users,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _roles = roles;
        _users = users;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync();
        await SeedSectorsAsync(cancellationToken);
        await SeedGovernoratesAsync(cancellationToken);
        await SeedPlansAsync(cancellationToken);
        await SeedAdministratorAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var (name, description) in AppRoles.All)
        {
            if (await _roles.RoleExistsAsync(name)) continue;

            var result = await _roles.CreateAsync(new AppRole(name, description));

            if (!result.Succeeded)
                _logger.LogError("Could not create role {Role}: {Errors}", name, Describe(result));
        }
    }

    private async Task SeedSectorsAsync(CancellationToken cancellationToken)
    {
        var existing = await _db.Sectors
            .Select(s => s.Code)
            .ToListAsync(cancellationToken);

        var missing = ReferenceData.Sectors
            .Where(s => !existing.Contains(s.Code))
            .Select(s => new Sector { Code = s.Code, NameEn = s.NameEn, NameAr = s.NameAr })
            .ToList();

        if (missing.Count == 0) return;

        _db.Sectors.AddRange(missing);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} sectors", missing.Count);
    }

    private async Task SeedGovernoratesAsync(CancellationToken cancellationToken)
    {
        var existing = await _db.Governorates
            .Include(g => g.Wilayats)
            .ToDictionaryAsync(g => g.Code, cancellationToken);

        var added = 0;

        foreach (var seed in ReferenceData.Governorates)
        {
            if (!existing.TryGetValue(seed.Code, out var governorate))
            {
                governorate = new Governorate
                {
                    Code = seed.Code,
                    NameEn = seed.NameEn,
                    NameAr = seed.NameAr
                };

                _db.Governorates.Add(governorate);
                added++;
            }

            var knownWilayats = governorate.Wilayats
                .Select(w => w.NameEn)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var wilayat in seed.Wilayats.Where(w => !knownWilayats.Contains(w.NameEn)))
            {
                governorate.Wilayats.Add(new Wilayat
                {
                    NameEn = wilayat.NameEn,
                    NameAr = wilayat.NameAr
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (added > 0) _logger.LogInformation("Seeded {Count} governorates", added);
    }

    private async Task SeedPlansAsync(CancellationToken cancellationToken)
    {
        var existing = await _db.SubscriptionPlans
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var missing = ReferenceData.Plans.Where(p => !existing.Contains(p.Code)).ToList();
        if (missing.Count == 0) return;

        foreach (var seed in missing)
        {
            var plan = new SubscriptionPlan
            {
                Code = seed.Code,
                Name = seed.Name,
                NameAr = seed.NameAr,
                Description = seed.Description,
                MonthlyPrice = seed.MonthlyPrice,
                PriceLabel = seed.PriceLabel,
                MonthlyBidLimit = seed.MonthlyBidLimit,
                IsHighlighted = seed.IsHighlighted,
                SortOrder = seed.SortOrder
            };

            for (var i = 0; i < seed.Features.Length; i++)
                plan.Features.Add(new SubscriptionPlanFeature { Label = seed.Features[i], SortOrder = i });

            _db.SubscriptionPlans.Add(plan);
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} subscription plans", missing.Count);
    }

    private async Task SeedAdministratorAsync()
    {
        var email = _configuration["Seed:AdministratorEmail"];
        var password = _configuration["Seed:AdministratorPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation(
                "No administrator seed configured; set Seed:AdministratorEmail and Seed:AdministratorPassword to create one");
            return;
        }

        if (await _users.FindByEmailAsync(email) is not null) return;

        var administrator = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = _configuration["Seed:AdministratorName"] ?? "Platform Administrator",
            AccountType = AccountType.Individual,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _users.CreateAsync(administrator, password);

        if (!created.Succeeded)
        {
            _logger.LogError("Could not create the administrator account: {Errors}", Describe(created));
            return;
        }

        await _users.AddToRoleAsync(administrator, AppRoles.Administrator);
        _logger.LogInformation("Created the administrator account {Email}", email);
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));
}
