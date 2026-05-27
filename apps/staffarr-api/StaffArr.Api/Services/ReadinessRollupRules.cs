namespace StaffArr.Api.Services;

public static class ReadinessRollupRules
{
    public const string TeamScope = "team";
    public const string SiteScope = "site";

    public static readonly IReadOnlySet<string> SupportedScopeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        TeamScope,
        SiteScope
    };

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? 1, 1, 168);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static decimal ComputeReadyPercent(int totalMembers, int readyCount)
    {
        if (totalMembers <= 0)
        {
            return 0m;
        }

        return Math.Round(readyCount * 100m / totalMembers, 1);
    }

    public static (int ReadyCount, int NotReadyCount, int OverrideCount) AggregateCounts(
        IReadOnlyList<PersonReadinessRollupSnapshot> members)
    {
        var readyCount = 0;
        var notReadyCount = 0;
        var overrideCount = 0;

        foreach (var member in members)
        {
            if (member.HasActiveOverride)
            {
                overrideCount++;
            }

            if (string.Equals(member.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
            {
                readyCount++;
            }
            else
            {
                notReadyCount++;
            }
        }

        return (readyCount, notReadyCount, overrideCount);
    }
}

public sealed record PersonReadinessRollupSnapshot(
    Guid PersonId,
    string ReadinessStatus,
    bool HasActiveOverride);
