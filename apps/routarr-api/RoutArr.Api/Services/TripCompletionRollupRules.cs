using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class TripCompletionRollupRules
{
    public const int DefaultReadStalenessHours = TripCompletionRollupDefaults.StalenessHours;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? TripCompletionRollupDefaults.StalenessHours, 1, 168);

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

    public static bool IsPending(
        DateTimeOffset tripUpdatedAt,
        DateTimeOffset? sourceUpdatedAt,
        DateTimeOffset? computedAt,
        DateTimeOffset asOfUtc,
        int stalenessHours)
    {
        if (computedAt is null || sourceUpdatedAt is null)
        {
            return true;
        }

        if (tripUpdatedAt > sourceUpdatedAt)
        {
            return true;
        }

        return IsStale(computedAt, asOfUtc, stalenessHours);
    }

    public static int? ComputeDurationMinutes(DateTimeOffset? startedAt, DateTimeOffset? completedAt)
    {
        if (startedAt is null || completedAt is null || completedAt < startedAt)
        {
            return null;
        }

        return (int)Math.Round((completedAt.Value - startedAt.Value).TotalMinutes);
    }

    public static bool IsTerminalTrip(string dispatchStatus) =>
        TripTerminalDispatchStatuses.All.Contains(dispatchStatus);
}
