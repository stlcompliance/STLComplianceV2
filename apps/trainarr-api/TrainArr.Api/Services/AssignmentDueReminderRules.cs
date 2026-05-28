using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public static class AssignmentDueReminderRules
{
    public static readonly string[] OpenAssignmentStatuses = ["assigned", "in_progress"];

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeDueSoonLeadDays(int? days) =>
        Math.Clamp(days ?? AssignmentReminderEscalationDefaults.DueSoonLeadDays, 1, 90);

    public static int NormalizeCooldownHours(int? hours) =>
        Math.Clamp(hours ?? AssignmentReminderEscalationDefaults.ReminderCooldownHours, 1, 168);

    public static int NormalizeMaxReminders(int? maxReminders) =>
        Math.Clamp(maxReminders ?? AssignmentReminderEscalationDefaults.MaxRemindersPerAssignment, 1, 50);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsDueForReminder(
        DateTimeOffset dueAt,
        DateTimeOffset? lastReminderSentAt,
        int dueSoonLeadDays,
        int cooldownHours,
        int reminderCount,
        int maxReminders,
        DateTimeOffset asOfUtc)
    {
        if (reminderCount >= maxReminders)
        {
            return false;
        }

        if (asOfUtc >= dueAt)
        {
            return false;
        }

        var windowStart = dueAt.AddDays(-dueSoonLeadDays);
        if (asOfUtc < windowStart)
        {
            return false;
        }

        if (lastReminderSentAt is null)
        {
            return true;
        }

        return asOfUtc >= lastReminderSentAt.Value.AddHours(cooldownHours);
    }

    public static double ComputeHoursUntilDue(DateTimeOffset dueAt, DateTimeOffset asOfUtc) =>
        Math.Max(0, (dueAt - asOfUtc).TotalHours);

    public static double? ComputeHoursUntilNextReminder(
        DateTimeOffset dueAt,
        DateTimeOffset? lastReminderSentAt,
        int dueSoonLeadDays,
        int cooldownHours,
        int reminderCount,
        int maxReminders,
        DateTimeOffset asOfUtc)
    {
        if (reminderCount >= maxReminders || asOfUtc >= dueAt)
        {
            return null;
        }

        var windowStart = dueAt.AddDays(-dueSoonLeadDays);
        if (asOfUtc < windowStart)
        {
            return (windowStart - asOfUtc).TotalHours;
        }

        if (lastReminderSentAt is null)
        {
            return 0;
        }

        var nextAt = lastReminderSentAt.Value.AddHours(cooldownHours);
        var remaining = (nextAt - asOfUtc).TotalHours;
        return remaining <= 0 ? 0 : remaining;
    }
}

public sealed record TenantAssignmentDueReminderSettingsSnapshot(
    bool IsEnabled,
    int DueSoonLeadDays,
    int ReminderCooldownHours,
    int MaxRemindersPerAssignment);
