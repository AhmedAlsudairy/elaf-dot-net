using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Infrastructure.Persistence;
using Munaqasat.Web.Infrastructure.Persistence.Seed;
using Munaqasat.Web.Services;
using Munaqasat.Web.Services.Accounts;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Companies;
using Munaqasat.Web.Services.Localization;
using Munaqasat.Web.Services.Tenders;
using Npgsql;

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

        // Postgres enums (text_direction, request_status) must be mapped on the data source as well as the model.
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<TextDirection>("text_direction");
        dataSourceBuilder.MapEnum<RequestStatus>("request_status");
        var dataSource = dataSourceBuilder.Build();
        services.AddSingleton(dataSource);

        services.AddSingleton<IClock, SystemClock>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options
                .UseNpgsql(dataSource, npgsql => npgsql.MigrationsHistoryTable("__migrations_history"))
                .UseSnakeCaseNamingConvention();

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
            .AddIdentity<IdentityUser, IdentityRole>(options =>
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
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<ProfileClaimsPrincipalFactory>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/SignIn";
            options.LogoutPath = "/Account/SignOut";
            options.AccessDeniedPath = "/Account/SignIn";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole(Roles.Administrator));
            options.AddPolicy(AuthorizationPolicies.TenderIssuer, policy => policy.RequireRole(Roles.Client, Roles.Administrator));
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<IRequestLanguage, RequestLanguage>();

        services.AddScoped<ITenderService, TenderService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}

public static class AuthorizationPolicies
{
    public const string Administrator = "Administrator";
    public const string TenderIssuer = "TenderIssuer";
}
