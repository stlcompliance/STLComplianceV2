namespace MaintainArr.Api.Services;

public static class AssetStatusRollupRules
{
    public static readonly IReadOnlySet<string> SupportedScopeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Entities.AssetStatusRollupScopeTypes.Fleet,
        Entities.AssetStatusRollupScopeTypes.AssetType,
        Entities.AssetStatusRollupScopeTypes.AssetClass,
        Entities.AssetStatusRollupScopeTypes.Site,
    };

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? Entities.AssetStatusRollupDefaults.StalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static decimal ComputeReadyPercent(int totalAssets, int readyCount)
    {
        if (totalAssets <= 0)
        {
            return 0m;
        }

        return Math.Round(readyCount * 100m / totalAssets, 1);
    }

    public static (int ReadyCount, int NotReadyCount) AggregateAssetCounts(
        IReadOnlyList<AssetStatusRollupSnapshot> assets)
    {
        var readyCount = 0;
        var notReadyCount = 0;

        foreach (var asset in assets)
        {
            if (string.Equals(asset.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
            {
                readyCount++;
            }
            else
            {
                notReadyCount++;
            }
        }

        return (readyCount, notReadyCount);
    }

    public static string NormalizeSiteKey(string? siteRef)
    {
        if (string.IsNullOrWhiteSpace(siteRef))
        {
            return string.Empty;
        }

        return siteRef.Trim();
    }
}

public sealed record AssetStatusRollupSnapshot(
    Guid AssetId,
    string ReadinessStatus);
