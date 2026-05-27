using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class ScheduledRuleEvaluationRules
{
    public const int DefaultIntervalHours = 24;

    public const int MinIntervalHours = 1;

    public const int MaxIntervalHours = 24 * 30;

    public static int NormalizeIntervalHours(int? intervalHours)
    {
        if (!intervalHours.HasValue)
        {
            return DefaultIntervalHours;
        }

        return Math.Clamp(intervalHours.Value, MinIntervalHours, MaxIntervalHours);
    }

    public static int NormalizeBatchSize(int batchSize) => Math.Clamp(batchSize, 1, 500);

    public static bool IsEligibleForScheduledEvaluation(string status, string? ruleContentJson) =>
        string.Equals(status, RulePackStatuses.Published, StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(ruleContentJson);

    public static bool IsDue(
        DateTimeOffset? lastScheduledEvaluationAt,
        DateTimeOffset asOfUtc,
        int intervalHours) =>
        lastScheduledEvaluationAt is null
        || lastScheduledEvaluationAt.Value.AddHours(intervalHours) <= asOfUtc;
}
