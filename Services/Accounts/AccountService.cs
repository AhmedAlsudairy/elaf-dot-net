using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Infrastructure.Persistence;
using UserRow = Munaqasat.Web.Models.UserRow;
using Munaqasat.Web.Services.Common;
using Munaqasat.Web.Services.Localization;

namespace Munaqasat.Web.Services.Accounts;

public record SignUpInput(string AccountType, string FullName, string? OrgName, string Email, string? Phone, string Password);

public record ProfileInput(string Name, string Email, string? Phone, string Timezone, string? Bio);

public record SettingsView(UserProfile Profile, Company? Company, string CompanyTitle, string CompanyAddress, string CompanyBio);

public interface IAccountService
{
    Task<OperationResult> SignUpAsync(SignUpInput input);
    Task<OperationResult> SignInAsync(string email, string password);
    Task SignOutAsync();

    Task<SettingsView?> GetSettingsAsync();
    Task<OperationResult> UpdateProfileAsync(ProfileInput input);
    Task<OperationResult> SetLanguageAsync(string languageCode, string timezone);
    Task<OperationResult> ChangePasswordAsync(string newPassword, string confirmPassword);

    Task<List<UserRow>> ListUsersAsync();
}

public sealed class AccountService : IAccountService
{
    /// <summary>Frontend role keys (the layout's role switcher) mapped to Identity roles.</summary>
    public static readonly IReadOnlyDictionary<string, string> RoleByKey = new Dictionary<string, string>
    {
        ["client"] = Roles.Client,
        ["contractor"] = Roles.Contractor,
        ["individual"] = Roles.Individual,
        ["admin"] = Roles.Administrator
    };

    public static readonly IReadOnlyDictionary<string, string> RoleLabels = new Dictionary<string, string>
    {
        ["client"] = "Client admin",
        ["contractor"] = "Contractor",
        ["individual"] = "Individual",
        ["admin"] = "Platform admin"
    };

    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly ICurrentUser _current;
    private readonly IRequestLanguage _language;

    public AccountService(
        ApplicationDbContext db,
        UserManager<IdentityUser> users,
        SignInManager<IdentityUser> signIn,
        ICurrentUser current,
        IRequestLanguage language)
    {
        _db = db;
        _users = users;
        _signIn = signIn;
        _current = current;
        _language = language;
    }

    public async Task<OperationResult> SignUpAsync(SignUpInput input)
    {
        var roleKey = input.AccountType is "contractor" or "individual" ? input.AccountType : "client";
        var email = input.Email.Trim();

        if (string.IsNullOrWhiteSpace(input.FullName) || string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("Name and email are required");

        if (await _db.UserProfiles.AnyAsync(p => p.Email == email))
            return OperationResult.Failure("An account with this email already exists");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var authUser = new IdentityUser { UserName = email, Email = email, PhoneNumber = input.Phone };
        var created = await _users.CreateAsync(authUser, input.Password);
        if (!created.Succeeded)
            return OperationResult.Failure(string.Join(" ", created.Errors.Select(e => e.Description)));

        await _users.AddToRoleAsync(authUser, RoleByKey[roleKey]);

        Company? company = null;
        if (roleKey != "individual" && !string.IsNullOrWhiteSpace(input.OrgName))
        {
            if (await _db.Companies.AnyAsync(c => c.CompanyEmail == email))
                return OperationResult.Failure("A company with this email already exists");

            company = new Company
            {
                CompanyEmail = email,
                PhoneNumber = input.Phone,
                DefaultLanguage = _language.Code,
                Sectors = new List<string>()
            };
            company.Translations.Add(new CompanyTranslation { LanguageCode = _language.Code, CompanyTitle = input.OrgName.Trim() });
            _db.Companies.Add(company);
        }

        _db.UserProfiles.Add(new UserProfile
        {
            Name = input.FullName.Trim(),
            Email = email,
            PhoneNumber = input.Phone,
            Role = roleKey,
            ClerkUserId = authUser.Id,
            Company = company,
            PreferredLanguage = _language.Code
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        await _signIn.SignInAsync(authUser, isPersistent: true);
        return OperationResult.Success();
    }

    public async Task<OperationResult> SignInAsync(string email, string password)
    {
        var result = await _signIn.PasswordSignInAsync(email.Trim(), password, isPersistent: true, lockoutOnFailure: true);
        if (result.IsLockedOut) return OperationResult.Failure("Too many attempts. Try again in 15 minutes");
        return result.Succeeded ? OperationResult.Success() : OperationResult.Failure("Email or password is incorrect");
    }

    public Task SignOutAsync() => _signIn.SignOutAsync();

    public async Task<SettingsView?> GetSettingsAsync()
    {
        var profile = await CurrentProfileQuery()
            .Include(p => p.Company!).ThenInclude(c => c.Translations)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (profile is null) return null;

        var tr = profile.Company is null ? null
            : Translations.Pick(profile.Company.Translations, _language.Code, profile.Company.DefaultLanguage);

        return new SettingsView(profile, profile.Company, tr?.CompanyTitle ?? "", tr?.Address ?? "", tr?.Bio ?? "");
    }

    public async Task<OperationResult> UpdateProfileAsync(ProfileInput input)
    {
        var profile = await CurrentProfileQuery().FirstOrDefaultAsync();
        if (profile is null) return OperationResult.Failure("Sign in to update your profile");

        var email = input.Email.Trim();
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("Name and email are required");

        if (email != profile.Email)
        {
            if (await _db.UserProfiles.AnyAsync(p => p.Email == email && p.Id != profile.Id))
                return OperationResult.Failure("That email is already used by another account");

            var authUser = await _users.FindByIdAsync(profile.ClerkUserId);
            if (authUser is not null)
            {
                await _users.SetEmailAsync(authUser, email);
                await _users.SetUserNameAsync(authUser, email);
            }
        }

        profile.Name = input.Name.Trim();
        profile.Email = email;
        profile.PhoneNumber = input.Phone;
        profile.Timezone = string.IsNullOrWhiteSpace(input.Timezone) ? profile.Timezone : input.Timezone;
        profile.Bio = input.Bio;
        await _db.SaveChangesAsync();
        await RefreshSignInAsync(profile.ClerkUserId);
        return OperationResult.Success();
    }

    public async Task<OperationResult> SetLanguageAsync(string languageCode, string timezone)
    {
        var profile = await CurrentProfileQuery().FirstOrDefaultAsync();
        if (profile is null) return OperationResult.Failure("Sign in to save your language");
        if (!await _db.Languages.AnyAsync(l => l.Code == languageCode && l.IsActive))
            return OperationResult.Failure("That language is not available");

        profile.PreferredLanguage = languageCode;
        if (!string.IsNullOrWhiteSpace(timezone)) profile.Timezone = timezone;
        await _db.SaveChangesAsync();
        await RefreshSignInAsync(profile.ClerkUserId);
        return OperationResult.Success();
    }

    public async Task<OperationResult> ChangePasswordAsync(string newPassword, string confirmPassword)
    {
        if (string.IsNullOrEmpty(newPassword)) return OperationResult.Failure("Enter a new password");
        if (newPassword != confirmPassword) return OperationResult.Failure("The passwords do not match");

        var authUser = _current.AuthUserId is null ? null : await _users.FindByIdAsync(_current.AuthUserId);
        if (authUser is null) return OperationResult.Failure("Sign in to change your password");

        var token = await _users.GeneratePasswordResetTokenAsync(authUser);
        var result = await _users.ResetPasswordAsync(authUser, token, newPassword);
        if (!result.Succeeded) return OperationResult.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));

        await _signIn.RefreshSignInAsync(authUser);
        return OperationResult.Success();
    }

    public async Task<List<UserRow>> ListUsersAsync()
    {
        var lang = _language.Code;
        var rows = await _db.UserProfiles
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Name,
                p.Email,
                p.Role,
                p.CreatedAt,
                Company = p.Company == null ? null : p.Company.Translations.ToList(),
                CompanyDefault = p.Company == null ? null : p.Company.DefaultLanguage
            })
            .ToListAsync();

        return rows.Select(r => new UserRow
        {
            Name = r.Name,
            Email = r.Email,
            Role = r.Role is not null && RoleLabels.TryGetValue(r.Role, out var label) ? label : r.Role ?? "",
            Company = r.Company is null ? "" : Translations.Pick(r.Company, lang, r.CompanyDefault)?.CompanyTitle ?? "",
            Joined = DateOnly.FromDateTime(r.CreatedAt.ToOffset(SystemClock.OmanOffset).DateTime)
        }).ToList();
    }

    private IQueryable<UserProfile> CurrentProfileQuery()
    {
        var authUserId = _current.AuthUserId ?? "";
        return _db.UserProfiles.Where(p => p.ClerkUserId == authUserId);
    }

    private async Task RefreshSignInAsync(string authUserId)
    {
        var authUser = await _users.FindByIdAsync(authUserId);
        if (authUser is not null) await _signIn.RefreshSignInAsync(authUser);
    }
}

/// <summary>Adds profile, company, role key and language claims to the auth cookie.</summary>
public sealed class ProfileClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
{
    private readonly ApplicationDbContext _db;

    public ProfileClaimsPrincipalFactory(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options,
        ApplicationDbContext db) : base(userManager, roleManager, options) => _db = db;

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var profile = await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.ClerkUserId == user.Id);
        if (profile is null) return identity;

        identity.AddClaim(new Claim(HttpContextCurrentUser.ProfileIdClaim, profile.Id));
        identity.AddClaim(new Claim(HttpContextCurrentUser.DisplayNameClaim, profile.Name));
        identity.AddClaim(new Claim(HttpContextCurrentUser.LanguageClaim, profile.PreferredLanguage));
        identity.AddClaim(new Claim(HttpContextCurrentUser.RoleKeyClaim, profile.Role ?? "individual"));
        if (profile.CompanyId is not null)
            identity.AddClaim(new Claim(HttpContextCurrentUser.CompanyIdClaim, profile.CompanyId));

        return identity;
    }
}
