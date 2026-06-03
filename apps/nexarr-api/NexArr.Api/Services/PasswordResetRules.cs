using System.Text.RegularExpressions;

namespace NexArr.Api.Services;

public static class PasswordResetRules
{
    public const int TokenLifetimeMinutes = 60;

    public const int DefaultMinPasswordLength = 12;
    public const bool DefaultRequirePasswordComplexity = true;

    private static readonly Regex PasswordComplexity = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        RegexOptions.Compiled);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static bool MeetsPasswordPolicy(string password) =>
        MeetsPasswordPolicy(password, ResolveConfiguredMinPasswordLength(), ResolveConfiguredRequirePasswordComplexity());

    public static bool MeetsPasswordPolicy(string password, int minPasswordLength, bool requireComplexity) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= minPasswordLength
        && (!requireComplexity || PasswordComplexity.IsMatch(password));

    public static string PasswordPolicyMessage() =>
        PasswordPolicyMessage(ResolveConfiguredMinPasswordLength(), ResolveConfiguredRequirePasswordComplexity());

    public static string PasswordPolicyMessage(int minPasswordLength, bool requireComplexity) =>
        requireComplexity
            ? $"Password must be at least {minPasswordLength} characters and include uppercase, lowercase, and a digit."
            : $"Password must be at least {minPasswordLength} characters.";

    public static int ResolveConfiguredMinPasswordLength()
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

    public static bool ResolveConfiguredRequirePasswordComplexity()
    {
        var configuredValue =
            Environment.GetEnvironmentVariable("AUTH_REQUIRE_PASSWORD_COMPLEXITY")
            ?? Environment.GetEnvironmentVariable("Auth__RequirePasswordComplexity");

        return !bool.TryParse(configuredValue, out var requireComplexity) || requireComplexity;
    }
}
