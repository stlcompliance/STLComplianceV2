namespace TrainArr.Api.Services;

public static class RecertificationAssignmentRules
{
    public const int DefaultLeadDays = 30;

    public static int NormalizeLeadDays(int? leadDays) =>
        Math.Clamp(leadDays ?? DefaultLeadDays, 1, 365);

    public static int NormalizeBatchSize(int batchSize) =>
        batchSize is < 1 or > 500 ? 100 : batchSize;

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 25, 1, 100);

    public static bool ShouldAssign(
        string status,
        DateTimeOffset? issueExpiresAt,
        DateTimeOffset? grantPublicationExpiresAt,
        DateTimeOffset asOfUtc,
        int leadDays)
    {
        if (!QualificationExpirationRules.IsExpirableStatus(status))
        {
            return false;
        }

        var effectiveExpiresAt = QualificationExpirationRules.ResolveEffectiveExpiresAt(
            issueExpiresAt,
            grantPublicationExpiresAt);
        if (effectiveExpiresAt is null)
        {
            return false;
        }

        if (effectiveExpiresAt <= asOfUtc)
        {
            return false;
        }

        var windowEnd = asOfUtc.AddDays(NormalizeLeadDays(leadDays));
        return effectiveExpiresAt <= windowEnd;
    }
}
