namespace StaffArr.Api.Services;

public static class PermissionProjectionRules
{
    public const int DefaultReadStalenessHours = 1;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 100, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? DefaultReadStalenessHours, 1, 168);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static string BuildPermissionIdentity(string permissionKey, string scopeType, string? scopeValue) =>
        $"{permissionKey}|{scopeType}|{scopeValue}";
}
