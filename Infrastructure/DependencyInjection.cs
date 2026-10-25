using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Infrastructure.Persistence.Seed;
using Munaqasat.Web.Services;
using Munaqasat.Web.Services.Bids;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Companies;
using Munaqasat.Web.Services.Matching;
using Munaqasat.Web.Services.Reviews;
using Munaqasat.Web.Services.Tenders;

namespace Munaqasat.Web.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default is not set. Run "
                + "dotnet user-secrets set \"ConnectionStrings:Default\" \"<connection string>\" for local "
                + "development, or set the ConnectionStrings__Default environment variable in hosting. "
                + "Never commit it to appsettings.json.");
        }

        services.AddSingleton<IClock, SystemClock>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__MigrationsHistory"));

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static IServiceCollection AddApplicationIdentity(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var enforceStrongPasswords = !environment.IsDevelopment();

        services
            .AddIdentity<AppUser, AppRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;

                options.Password.RequiredLength = enforceStrongPasswords ? 10 : 6;
                options.Password.RequireDigit = enforceStrongPasswords;
                options.Password.RequireUppercase = enforceStrongPasswords;
                options.Password.RequireLowercase = enforceStrongPasswords;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/SignIn";
            options.LogoutPath = "/Account/SignOut";
            options.AccessDeniedPath = "/Account/Denied";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Administrator, policy =>
                policy.RequireRole(AppRoles.Administrator));

            options.AddPolicy(AuthorizationPolicies.CompanyMember, policy =>
                policy.RequireRole(AppRoles.CompanyOwner, AppRoles.CompanyMember, AppRoles.Administrator));

            options.AddPolicy(AuthorizationPolicies.CompanyOwner, policy =>
                policy.RequireRole(AppRoles.CompanyOwner, AppRoles.Administrator));
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddScoped<ITenderService, TenderService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<IReviewService, ReviewService>();

        services.AddSingleton<ITenderMatchScorer, KeywordTenderMatchScorer>();
        services.AddScoped<ITenderMatchService, TenderMatchService>();

        return services;
    }
}

public static class AuthorizationPolicies
{
    public const string Administrator = "Administrator";
    public const string CompanyMember = "CompanyMember";
    public const string CompanyOwner = "CompanyOwner";
}
