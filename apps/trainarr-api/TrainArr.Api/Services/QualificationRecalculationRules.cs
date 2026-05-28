namespace TrainArr.Api.Services;

using TrainArr.Api.Contracts;

public static class QualificationRecalculationRules
{
    public const int DefaultStalenessHours = 24;

    public static readonly IReadOnlySet<string> RecalculableStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "issued",
        "suspended",
    };

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 100, 1, 500);

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

    public static bool ShouldAutoSuspend(
        bool autoSuspendOnBlock,
        string issueStatus,
        string checkOutcome,
        string? complianceOutcome)
    {
        if (!autoSuspendOnBlock)
        {
            return false;
        }

        if (!string.Equals(issueStatus, "issued", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(checkOutcome, QualificationCheckOutcomes.Block, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(complianceOutcome)
            && string.Equals(complianceOutcome, QualificationCheckOutcomes.Block, StringComparison.OrdinalIgnoreCase);
    }
}
