using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class AssetReadinessRules
{
    public static bool IsOpenDefectStatus(string status) =>
        string.Equals(status, DefectStatuses.Open, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, DefectStatuses.Acknowledged, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, DefectStatuses.InRepair, StringComparison.OrdinalIgnoreCase);

    public static bool IsBlockingDefectSeverity(string severity) =>
        string.Equals(severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase)
        || string.Equals(severity, DefectSeverities.High, StringComparison.OrdinalIgnoreCase);

    public static bool IsActiveWorkOrderStatus(string status) =>
        WorkOrderStatuses.Active.Contains(status);

    public static bool IsBlockingPmDueStatus(string dueStatus) =>
        string.Equals(dueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)
        || string.Equals(dueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase);

    public static bool IsActivePmScheduleStatus(string status) =>
        string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);

    public static bool IsFailedInspectionResult(string? result) =>
        string.Equals(result, InspectionRunResults.Failed, StringComparison.OrdinalIgnoreCase);

    public static bool IsReady(int blockerCount) => blockerCount == 0;

    public static string ResolveReadinessBasis(bool isReady) =>
        isReady ? "maintenance_clear" : "maintenance_blockers";

    public static string ResolveReadinessStatus(bool isReady) => isReady ? "ready" : "not_ready";
}
