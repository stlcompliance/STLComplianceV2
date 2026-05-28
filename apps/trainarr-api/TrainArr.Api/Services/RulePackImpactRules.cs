namespace TrainArr.Api.Services;

public static class RulePackImpactRules
{
    public const int DefaultStalenessHours = 24;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 200);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? DefaultStalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static int NormalizeStateListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static bool ShouldAutoUpdateBaselines(
        bool autoUpdateRequirementBaselines,
        bool requiresAttention,
        bool packNotFound) =>
        autoUpdateRequirementBaselines && !requiresAttention && !packNotFound;
}
