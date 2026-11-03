using Munaqasat.Web.Infrastructure;
using Munaqasat.Web.Infrastructure.Persistence.Seed;

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

app.MapRazorPages();

app.Run();
