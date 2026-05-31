using System.Text.RegularExpressions;

namespace NexArr.Api.Services;

public static class PasswordResetRules
{
    public const int TokenLifetimeMinutes = 60;

    public const int DefaultMinPasswordLength = 12;

    private static readonly Regex PasswordComplexity = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        RegexOptions.Compiled);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static bool MeetsPasswordPolicy(string password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= ResolveMinPasswordLength()
        && (!ResolveRequirePasswordComplexity() || PasswordComplexity.IsMatch(password));

    public static string PasswordPolicyMessage() =>
        ResolveRequirePasswordComplexity()
            ? $"Password must be at least {ResolveMinPasswordLength()} characters and include uppercase, lowercase, and a digit."
            : $"Password must be at least {ResolveMinPasswordLength()} characters.";

    private static int ResolveMinPasswordLength()
    {
        var configuredValue =
            Environment.GetEnvironmentVariable("AUTH_PASSWORD_MIN_LENGTH")
            ?? Environment.GetEnvironmentVariable("Auth__PasswordMinLength");

        if (!int.TryParse(configuredValue, out var minPasswordLength) || minPasswordLength <= 0)
        {
            return DefaultMinPasswordLength;
        }

        return minPasswordLength;
    }

    private static bool ResolveRequirePasswordComplexity()
    {
        var configuredValue =
            Environment.GetEnvironmentVariable("AUTH_REQUIRE_PASSWORD_COMPLEXITY")
            ?? Environment.GetEnvironmentVariable("Auth__RequirePasswordComplexity");

        return !bool.TryParse(configuredValue, out var requireComplexity) || requireComplexity;
    }
}
