using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;

namespace Munaqasat.Web.Infrastructure.Persistence.Seed;

/// <summary>
/// Applies migrations, then seeds languages, roles and the administrator (always), and the
/// sample tenders / companies the UI was designed with (when <c>Seed:SampleData</c> is true).
/// Every step is idempotent.
/// </summary>
public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly UserManager<IdentityUser> _users;
    private readonly IConfiguration _config;
    private readonly ILogger<DatabaseSeeder> _log;

    public DatabaseSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roles,
        UserManager<IdentityUser> users,
        IConfiguration config,
        ILogger<DatabaseSeeder> log)
    {
        _db = db;
        _roles = roles;
        _users = users;
        _config = config;
        _log = log;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        // Reload enum/extension types created by the migration on this connection.
        await _db.Database.OpenConnectionAsync();
        await ((Npgsql.NpgsqlConnection)_db.Database.GetDbConnection()).ReloadTypesAsync();
        await _db.Database.CloseConnectionAsync();

        await SeedLanguagesAsync();
        await SeedRolesAsync();
        await SeedAdministratorAsync();

        if (_config.GetValue("Seed:SampleData", false))
            await SampleData.SeedAsync(_db, _users, _log);

        _log.LogInformation(
            "Seed complete: {Languages} languages, {Tenders} tenders, {Companies} companies, {Requests} requests, {Ratings} ratings",
            await _db.Languages.CountAsync(), await _db.Tenders.CountAsync(), await _db.Companies.CountAsync(),
            await _db.TenderRequests.CountAsync(), await _db.CompanyRatings.CountAsync());
    }

    private async Task SeedLanguagesAsync()
    {
        var languages = new[]
        {
            new Language { Code = "en", Name = "English", NativeName = "English", Direction = TextDirection.Ltr, IsDefault = true, SortOrder = 0 },
            new Language { Code = "ar", Name = "Arabic", NativeName = "العربية", Direction = TextDirection.Rtl, IsDefault = false, SortOrder = 1 }
        };

        foreach (var language in languages)
        {
            if (!await _db.Languages.AnyAsync(l => l.Code == language.Code)) _db.Languages.Add(language);
        }

        await _db.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await _roles.RoleExistsAsync(role)) await _roles.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedAdministratorAsync()
    {
        var email = _config["Seed:AdministratorEmail"];
        var password = _config["Seed:AdministratorPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _log.LogInformation("Seed:AdministratorEmail / Seed:AdministratorPassword not set; skipping administrator");
            return;
        }

        var authUser = await _users.FindByEmailAsync(email);
        if (authUser is null)
        {
            authUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _users.CreateAsync(authUser, password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Could not create administrator: " + string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        if (!await _users.IsInRoleAsync(authUser, Roles.Administrator))
            await _users.AddToRoleAsync(authUser, Roles.Administrator);

        if (!await _db.UserProfiles.AnyAsync(p => p.ClerkUserId == authUser.Id))
        {
            _db.UserProfiles.Add(new UserProfile
            {
                Name = _config["Seed:AdministratorName"] ?? "Platform administrator",
                Email = email,
                Role = "admin",
                ClerkUserId = authUser.Id
            });
            await _db.SaveChangesAsync();
        }
    }
}
