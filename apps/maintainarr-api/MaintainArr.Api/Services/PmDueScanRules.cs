using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class PmDueScanRules
{
    public const int DefaultOverdueGraceDays = 1;

    public static readonly HashSet<string> ScannableScheduleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active"
    };

    public static readonly HashSet<string> UpdatableDueStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PmDueStatuses.Scheduled,
        PmDueStatuses.Due
    };

    public static bool IsScannableScheduleStatus(string status) =>
        ScannableScheduleStatuses.Contains(status);

    public static bool ShouldMarkDue(
        string scheduleStatus,
        string dueStatus,
        DateTimeOffset nextDueAt,
        DateTimeOffset asOfUtc)
    {
        if (!IsScannableScheduleStatus(scheduleStatus))
        {
            return false;
        }

        return string.Equals(dueStatus, PmDueStatuses.Scheduled, StringComparison.OrdinalIgnoreCase)
            && nextDueAt <= asOfUtc;
    }

    public static bool ShouldMarkOverdue(
        string scheduleStatus,
        string dueStatus,
        DateTimeOffset nextDueAt,
        DateTimeOffset asOfUtc,
        int overdueGraceDays = DefaultOverdueGraceDays)
    {
        if (!IsScannableScheduleStatus(scheduleStatus))
        {
            return false;
        }

        if (!UpdatableDueStatuses.Contains(dueStatus))
        {
            return false;
        }

        var overdueThreshold = asOfUtc.AddDays(-Math.Max(0, overdueGraceDays));
        return nextDueAt < overdueThreshold;
    }

    public static string ResolveTargetDueStatus(
        string scheduleStatus,
        string dueStatus,
        DateTimeOffset nextDueAt,
        DateTimeOffset asOfUtc,
        int overdueGraceDays = DefaultOverdueGraceDays)
    {
        if (ShouldMarkOverdue(scheduleStatus, dueStatus, nextDueAt, asOfUtc, overdueGraceDays))
        {
            return PmDueStatuses.Overdue;
        }

        if (ShouldMarkDue(scheduleStatus, dueStatus, nextDueAt, asOfUtc))
        {
            return PmDueStatuses.Due;
        }

        return dueStatus;
    }
}
