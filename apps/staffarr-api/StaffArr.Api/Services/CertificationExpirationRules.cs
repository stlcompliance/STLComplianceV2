namespace StaffArr.Api.Services;

public static class CertificationExpirationRules
{
    public static readonly HashSet<string> ExpirableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active"
    };

    public static bool IsExpirableStatus(string status) => ExpirableStatuses.Contains(status);

    public static bool ShouldExpire(
        string status,
        DateTimeOffset? expiresAt,
        DateTimeOffset asOfUtc)
    {
        if (!IsExpirableStatus(status))
        {
            return false;
        }

        return expiresAt is not null && expiresAt <= asOfUtc;
    }
}
