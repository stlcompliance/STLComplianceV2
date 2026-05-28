namespace NexArr.Api.Services;

public static class ServiceTokenCleanupRules
{
    public const int DefaultRetentionDaysAfterExpiry = 7;
    public const int DefaultRetentionDaysAfterRevoke = 30;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 100, 1, 500);

    public static int NormalizeRetentionDaysAfterExpiry(int? retentionDays) =>
        Math.Clamp(retentionDays ?? DefaultRetentionDaysAfterExpiry, 0, 365);

    public static int NormalizeRetentionDaysAfterRevoke(int? retentionDays) =>
        Math.Clamp(retentionDays ?? DefaultRetentionDaysAfterRevoke, 0, 365);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static bool IsExpiredPurgeCandidate(
        DateTimeOffset? revokedAt,
        DateTimeOffset expiresAt,
        DateTimeOffset asOfUtc,
        int retentionDaysAfterExpiry)
    {
        if (revokedAt is not null)
        {
            return false;
        }

        var cutoff = asOfUtc.AddDays(-retentionDaysAfterExpiry);
        return expiresAt <= cutoff;
    }

    public static bool IsRevokedPurgeCandidate(
        DateTimeOffset? revokedAt,
        DateTimeOffset asOfUtc,
        int retentionDaysAfterRevoke)
    {
        if (revokedAt is null)
        {
            return false;
        }

        var cutoff = asOfUtc.AddDays(-retentionDaysAfterRevoke);
        return revokedAt.Value <= cutoff;
    }

    public static bool IsPurgeCandidate(
        DateTimeOffset? revokedAt,
        DateTimeOffset expiresAt,
        DateTimeOffset asOfUtc,
        int retentionDaysAfterExpiry,
        int retentionDaysAfterRevoke) =>
        IsRevokedPurgeCandidate(revokedAt, asOfUtc, retentionDaysAfterRevoke)
        || IsExpiredPurgeCandidate(revokedAt, expiresAt, asOfUtc, retentionDaysAfterExpiry);

    public static string ResolveCleanupReason(
        DateTimeOffset? revokedAt,
        DateTimeOffset expiresAt,
        DateTimeOffset asOfUtc,
        int retentionDaysAfterExpiry,
        int retentionDaysAfterRevoke)
    {
        if (IsRevokedPurgeCandidate(revokedAt, asOfUtc, retentionDaysAfterRevoke))
        {
            return "revoked";
        }

        if (IsExpiredPurgeCandidate(revokedAt, expiresAt, asOfUtc, retentionDaysAfterExpiry))
        {
            return "expired";
        }

        return "unknown";
    }
}
