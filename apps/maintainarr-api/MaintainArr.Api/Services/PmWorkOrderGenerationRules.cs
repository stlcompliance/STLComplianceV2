using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class PmWorkOrderGenerationRules
{
    public static bool ShouldEnsureWorkOrder(string dueStatus) =>
        string.Equals(dueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)
        || string.Equals(dueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase);

    public static string MapDueStatusToPriority(string dueStatus) =>
        string.Equals(dueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase)
            ? WorkOrderPriorities.High
            : WorkOrderPriorities.Medium;

    public static string BuildTitle(string scheduleName)
    {
        var normalized = scheduleName.Trim();
        return normalized.Length == 0 ? "Preventive maintenance" : $"PM: {normalized}";
    }

    public static string BuildDescription(string scheduleName, string scheduleDescription, DateTimeOffset nextDueAt)
    {
        var details = string.IsNullOrWhiteSpace(scheduleDescription)
            ? $"Scheduled preventive maintenance for {scheduleName.Trim()}."
            : scheduleDescription.Trim();

        return $"{details} Next due {nextDueAt:yyyy-MM-dd}.";
    }
}
