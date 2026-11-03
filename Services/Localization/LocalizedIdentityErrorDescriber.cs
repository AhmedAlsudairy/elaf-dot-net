using Microsoft.AspNetCore.Identity;

namespace Munaqasat.Web.Services.Localization;

/// <summary>Identity's sign-up and password errors, in the request language.</summary>
public sealed class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IRequestLanguage _language;

    public LocalizedIdentityErrorDescriber(IRequestLanguage language) => _language = language;

    private IdentityError Error(IdentityError original, string english, params object?[] args) =>
        new() { Code = original.Code, Description = _language.T(english, args) };

    public override IdentityError DuplicateEmail(string email) =>
        Error(base.DuplicateEmail(email), "An account with this email already exists");

    public override IdentityError DuplicateUserName(string userName) =>
        Error(base.DuplicateUserName(userName), "An account with this email already exists");

    public override IdentityError InvalidEmail(string? email) =>
        Error(base.InvalidEmail(email), "Enter a valid email address");

    public override IdentityError InvalidUserName(string? userName) =>
        Error(base.InvalidUserName(userName), "Enter a valid email address");

    public override IdentityError PasswordTooShort(int length) =>
        Error(base.PasswordTooShort(length), "Passwords must be at least {0} characters", length);

    public override IdentityError PasswordRequiresDigit() =>
        Error(base.PasswordRequiresDigit(), "Passwords must have at least one digit (0-9)");

    public override IdentityError PasswordRequiresLower() =>
        Error(base.PasswordRequiresLower(), "Passwords must have at least one lowercase letter (a-z)");

    public override IdentityError PasswordRequiresUpper() =>
        Error(base.PasswordRequiresUpper(), "Passwords must have at least one uppercase letter (A-Z)");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(base.PasswordRequiresNonAlphanumeric(), "Passwords must have at least one symbol");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Error(base.PasswordRequiresUniqueChars(uniqueChars), "Passwords must use at least {0} different characters", uniqueChars);

    public override IdentityError DefaultError() =>
        Error(base.DefaultError(), "Something went wrong. Please try again");
}
