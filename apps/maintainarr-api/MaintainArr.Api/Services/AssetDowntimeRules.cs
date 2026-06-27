using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class AssetDowntimeRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? AssetDowntimeDefaults.BatchSize, 1, 500);

    public static int NormalizeAvailabilityPeriodDays(int? periodDays) =>
        Math.Clamp(periodDays ?? AssetDowntimeDefaults.AvailabilityPeriodDays, 1, 365);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsManualReason(string reason) =>
        AssetDowntimeReasons.ManualReasons.Contains(reason);

    public static bool IsSupportedReason(string reason) =>
        IsManualReason(reason)
        || string.Equals(reason, AssetDowntimeReasons.OutOfService, StringComparison.OrdinalIgnoreCase)
        || string.Equals(reason, AssetDowntimeReasons.RestrictedUse, StringComparison.OrdinalIgnoreCase);

    public static bool IsAutomaticDowntimeState(
        string lifecycleStatus,
        string readinessStatus,
        bool trackOutOfService,
        bool trackNotReady)
    {
        if (trackOutOfService
            && string.Equals(lifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (trackNotReady
            && !string.Equals(readinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static string ResolveAutomaticReason(string lifecycleStatus, string readinessStatus)
    {
        if (string.Equals(lifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase))
        {
            return AssetDowntimeReasons.OutOfService;
        }

        return AssetDowntimeReasons.RestrictedUse;
    }

    public static string ResolveAutomaticStatusTrigger(string lifecycleStatus, string readinessStatus)
    {
        if (string.Equals(lifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase))
        {
            return $"lifecycle:{lifecycleStatus}";
        }

        return $"readiness:{readinessStatus}";
    }

    public static decimal ComputeAvailabilityPercent(decimal totalHours, decimal downtimeHours)
    {
        if (totalHours <= 0m)
        {
            return 100m;
        }

        var availableHours = Math.Max(0m, totalHours - downtimeHours);
        return Math.Round(availableHours * 100m / totalHours, 1);
    }

    public static decimal ComputeDowntimeHoursForPeriod(
        IReadOnlyList<DowntimeInterval> events,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd)
    {
        if (periodEnd <= periodStart || events.Count == 0)
        {
            return 0m;
        }

        var totalMinutes = 0d;
        foreach (var interval in events)
        {
            var start = interval.StartedAt < periodStart ? periodStart : interval.StartedAt;
            var end = interval.EndedAt ?? periodEnd;
            if (end > periodEnd)
            {
                end = periodEnd;
            }

            if (end <= start)
            {
                continue;
            }

            totalMinutes += (end - start).TotalMinutes;
        }

        return Math.Round((decimal)(totalMinutes / 60d), 2);
    }

    public static (decimal Planned, decimal Unplanned) SplitPlannedDowntimeHours(
        IReadOnlyList<DowntimeInterval> events,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd)
    {
        var planned = 0m;
        var unplanned = 0m;

        foreach (var interval in events)
        {
            var start = interval.StartedAt < periodStart ? periodStart : interval.StartedAt;
            var end = interval.EndedAt ?? periodEnd;
            if (end > periodEnd)
            {
                end = periodEnd;
            }

            if (end <= start)
            {
                continue;
            }

            var hours = (decimal)(end - start).TotalHours;
            if (interval.IsPlanned)
            {
                planned += hours;
            }
            else
            {
                unplanned += hours;
            }
        }

        return (Math.Round(planned, 2), Math.Round(unplanned, 2));
    }
}

public sealed record DowntimeInterval(
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    bool IsPlanned);
