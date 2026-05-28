using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public static class AssignmentEscalationRules
{
    public static readonly string[] OpenAssignmentStatuses = AssignmentDueReminderRules.OpenAssignmentStatuses;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeOverdueHours(int? hours) =>
        Math.Clamp(hours ?? AssignmentReminderEscalationDefaults.OverdueEscalationAfterHours, 1, 720);

    public static int NormalizeCooldownHours(int? hours) =>
        Math.Clamp(hours ?? AssignmentReminderEscalationDefaults.EscalationCooldownHours, 1, 168);

    public static int NormalizeMaxEscalations(int? maxEscalations) =>
        Math.Clamp(maxEscalations ?? AssignmentReminderEscalationDefaults.MaxEscalationsPerAssignment, 1, 50);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static int NormalizeEventListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsDueForEscalation(
        DateTimeOffset dueAt,
        DateTimeOffset? lastEscalatedAt,
        int overdueEscalationAfterHours,
        int cooldownHours,
        int escalationCount,
        int maxEscalations,
        DateTimeOffset asOfUtc)
    {
        if (escalationCount >= maxEscalations)
        {
            return false;
        }

        var overdueThreshold = dueAt.AddHours(overdueEscalationAfterHours);
        if (asOfUtc < overdueThreshold)
        {
            return false;
        }

        if (lastEscalatedAt is null)
        {
            return true;
        }

        return asOfUtc >= lastEscalatedAt.Value.AddHours(cooldownHours);
    }

    public static double ComputeHoursOverdue(DateTimeOffset dueAt, DateTimeOffset asOfUtc) =>
        Math.Max(0, (asOfUtc - dueAt).TotalHours);

    public static double? ComputeHoursUntilNextEscalation(
        DateTimeOffset dueAt,
        DateTimeOffset? lastEscalatedAt,
        int overdueEscalationAfterHours,
        int cooldownHours,
        int escalationCount,
        int maxEscalations,
        DateTimeOffset asOfUtc)
    {
        if (escalationCount >= maxEscalations)
        {
            return null;
        }

        var overdueThreshold = dueAt.AddHours(overdueEscalationAfterHours);
        if (asOfUtc < overdueThreshold)
        {
            return (overdueThreshold - asOfUtc).TotalHours;
        }

        if (lastEscalatedAt is null)
        {
            return 0;
        }

        var nextAt = lastEscalatedAt.Value.AddHours(cooldownHours);
        var remaining = (nextAt - asOfUtc).TotalHours;
        return remaining <= 0 ? 0 : remaining;
    }
}

public sealed record TenantAssignmentEscalationSettingsSnapshot(
    bool IsEnabled,
    int OverdueEscalationAfterHours,
    int EscalationCooldownHours,
    int MaxEscalationsPerAssignment);
