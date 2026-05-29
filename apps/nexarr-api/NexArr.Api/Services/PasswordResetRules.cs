using System.Text.RegularExpressions;

namespace NexArr.Api.Services;

public static class PasswordResetRules
{
    public const int TokenLifetimeMinutes = 60;

    public const int MinPasswordLength = 12;

    private static readonly Regex PasswordComplexity = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        RegexOptions.Compiled);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static bool MeetsPasswordPolicy(string password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= MinPasswordLength
        && PasswordComplexity.IsMatch(password);

    public static string PasswordPolicyMessage() =>
        $"Password must be at least {MinPasswordLength} characters and include uppercase, lowercase, and a digit.";
}
