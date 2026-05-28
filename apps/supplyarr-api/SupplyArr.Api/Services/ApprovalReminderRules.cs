using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class ApprovalReminderRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeThresholdHours(int? hours, int defaultHours) =>
        Math.Clamp(hours ?? defaultHours, 1, 720);

    public static int NormalizeCooldownHours(int? hours) =>
        Math.Clamp(hours ?? ApprovalReminderDefaults.ReminderCooldownHours, 1, 168);

    public static int NormalizeMaxReminders(int? maxReminders) =>
        Math.Clamp(maxReminders ?? ApprovalReminderDefaults.MaxRemindersPerSubject, 1, 100);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static int GetThresholdHours(
        TenantApprovalReminderSettingsSnapshot settings,
        string subjectType) =>
        string.Equals(subjectType, ApprovalReminderSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase)
            ? settings.PrReminderAfterHours
            : settings.PoReminderAfterHours;

    public static bool ShouldNotify(
        TenantApprovalReminderSettingsSnapshot settings,
        string subjectType) =>
        string.Equals(subjectType, ApprovalReminderSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase)
            ? settings.NotifyOnPrApprovalReminder
            : settings.NotifyOnPoApprovalReminder;

    public static string GetReminderEventKind(string subjectType) =>
        string.Equals(subjectType, ApprovalReminderSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase)
            ? ProcurementNotificationEventKinds.PurchaseRequestApprovalReminder
            : ProcurementNotificationEventKinds.PurchaseOrderApprovalReminder;

    public static string GetRelatedEntityType(string subjectType) =>
        string.Equals(subjectType, ApprovalReminderSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase)
            ? "purchase_request"
            : "purchase_order";

    public static bool IsDueForReminder(
        DateTimeOffset pendingSince,
        DateTimeOffset? lastReminderSentAt,
        int thresholdHours,
        int cooldownHours,
        int reminderCount,
        int maxReminders,
        DateTimeOffset asOfUtc)
    {
        if (reminderCount >= maxReminders)
        {
            return false;
        }

        var initialDueAt = pendingSince.AddHours(thresholdHours);
        if (lastReminderSentAt is null)
        {
            return asOfUtc >= initialDueAt;
        }

        var nextDueAt = lastReminderSentAt.Value.AddHours(cooldownHours);
        return asOfUtc >= nextDueAt && asOfUtc >= initialDueAt;
    }

    public static double ComputeHoursPending(DateTimeOffset pendingSince, DateTimeOffset asOfUtc) =>
        Math.Max(0, (asOfUtc - pendingSince).TotalHours);

    public static double? ComputeHoursUntilNextReminder(
        DateTimeOffset pendingSince,
        DateTimeOffset? lastReminderSentAt,
        int thresholdHours,
        int cooldownHours,
        int reminderCount,
        int maxReminders,
        DateTimeOffset asOfUtc)
    {
        if (reminderCount >= maxReminders)
        {
            return null;
        }

        var nextDueAt = lastReminderSentAt is null
            ? pendingSince.AddHours(thresholdHours)
            : DateTimeOffset.FromUnixTimeSeconds(
                Math.Max(
                    pendingSince.AddHours(thresholdHours).ToUnixTimeSeconds(),
                    lastReminderSentAt.Value.AddHours(cooldownHours).ToUnixTimeSeconds()));

        var remaining = (nextDueAt - asOfUtc).TotalHours;
        return remaining <= 0 ? 0 : remaining;
    }

    public static bool IsOverdue(
        DateTimeOffset pendingSince,
        int thresholdHours,
        DateTimeOffset asOfUtc) =>
        asOfUtc >= pendingSince.AddHours(thresholdHours);
}

public sealed record TenantApprovalReminderSettingsSnapshot(
    bool IsEnabled,
    int PrReminderAfterHours,
    int PoReminderAfterHours,
    int ReminderCooldownHours,
    int MaxRemindersPerSubject,
    bool NotifyOnPrApprovalReminder,
    bool NotifyOnPoApprovalReminder);
