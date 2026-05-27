namespace TrainArr.Api.Services;

public static class QualificationExpirationRules
{
    public static readonly HashSet<string> ExpirableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "issued",
        "suspended"
    };

    public static bool IsExpirableStatus(string status) => ExpirableStatuses.Contains(status);

    public static DateTimeOffset? ResolveEffectiveExpiresAt(
        DateTimeOffset? issueExpiresAt,
        DateTimeOffset? grantPublicationExpiresAt) =>
        issueExpiresAt ?? grantPublicationExpiresAt;

    public static bool ShouldExpire(
        string status,
        DateTimeOffset? issueExpiresAt,
        DateTimeOffset? grantPublicationExpiresAt,
        DateTimeOffset asOfUtc)
    {
        if (!IsExpirableStatus(status))
        {
            return false;
        }

        var effective = ResolveEffectiveExpiresAt(issueExpiresAt, grantPublicationExpiresAt);
        return effective is not null && effective <= asOfUtc;
    }
}
