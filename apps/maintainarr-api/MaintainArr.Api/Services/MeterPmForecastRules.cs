using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class MeterPmForecastRules
{
    public static bool ShouldMarkDueFromUsage(
        string scheduleStatus,
        string dueStatus,
        decimal currentReading,
        decimal? nextDueAtUsage)
    {
        if (!PmDueScanRules.IsScannableScheduleStatus(scheduleStatus))
        {
            return false;
        }

        if (!nextDueAtUsage.HasValue)
        {
            return false;
        }

        return string.Equals(dueStatus, PmDueStatuses.Scheduled, StringComparison.OrdinalIgnoreCase)
            && currentReading >= nextDueAtUsage.Value;
    }

    public static decimal? ComputeUsageUntilDue(decimal currentReading, decimal? nextDueAtUsage)
    {
        if (!nextDueAtUsage.HasValue)
        {
            return null;
        }

        var remaining = nextDueAtUsage.Value - currentReading;
        return remaining > 0 ? remaining : 0;
    }

    public static decimal ComputeInitialNextDueAtUsage(decimal baselineReading, decimal intervalUsage) =>
        baselineReading + intervalUsage;

    public static decimal ComputeNextDueAtUsageAfterCompletion(
        decimal completedAtUsage,
        decimal intervalUsage) =>
        completedAtUsage + intervalUsage;
}
