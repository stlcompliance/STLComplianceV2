using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public static class OffboardingRules
{
    private static readonly HashSet<string> AllowedTargetEmploymentStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "inactive", "terminated" };

    public static string NormalizeTargetEmploymentStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!AllowedTargetEmploymentStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "offboarding.validation",
                "Target employment status must be inactive or terminated.",
                400);
        }

        return normalized;
    }

    public static string? NormalizeSeparationReason(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 512)
        {
            throw new StlApiException(
                "offboarding.validation",
                "Separation reason must be 512 characters or fewer.",
                400);
        }

        return trimmed;
    }

    public static void ValidateSeparationDate(DateTimeOffset separationDate)
    {
        if (separationDate == default)
        {
            throw new StlApiException(
                "offboarding.validation",
                "Separation date is required.",
                400);
        }
    }
}
