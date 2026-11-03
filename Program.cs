using Munaqasat.Web.Infrastructure;
using Munaqasat.Web.Infrastructure.Persistence.Seed;
using Munaqasat.Web.Services.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Settings");
    options.Conventions.AuthorizePage("/Tenders/Create", AuthorizationPolicies.TenderIssuer);
    options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.Administrator);
});

// Emit Arabic (and any other script) as-is instead of &#x...; entities.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));

builder.Services.AddPersistence(builder.Configuration, builder.Environment);
builder.Services.AddApplicationIdentity(builder.Environment);
builder.Services.AddApplicationServices();

var app = builder.Build();

if (app.Configuration.GetValue("Seed:RunOnStartup", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ?lang=ar|en switches the interface language and is remembered in a cookie.
// The request culture follows it, so dates show Arabic month names in Arabic.
app.Use(async (context, next) =>
{
    var requested = context.Request.Query["lang"].ToString();
    if (RequestLanguage.Supported.Contains(requested))
    {
        context.Response.Cookies.Append(RequestLanguage.CookieName, requested,
            new CookieOptions { MaxAge = TimeSpan.FromDays(365), IsEssential = true, SameSite = SameSiteMode.Lax });
    }

    var culture = RequestLanguage.Culture(context.RequestServices.GetRequiredService<IRequestLanguage>().Code);
    System.Globalization.CultureInfo.CurrentCulture = culture;
    System.Globalization.CultureInfo.CurrentUICulture = culture;
    await next();
});

app.MapRazorPages();

app.Run();
