namespace SupplyArr.Api.Services;

public static class SupplyReadinessRules
{
    private static readonly HashSet<string> BlockedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "blocked",
        "restricted",
        "inactive",
    };

    private static readonly HashSet<string> PendingApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "pending_review",
        "pending_onboarding",
    };

    public static bool IsReady(int blockerCount) => blockerCount == 0;

    public static string ResolveReadinessStatus(bool isReady) => isReady ? "ready" : "not_ready";

    public static string ResolveReadinessBasis(bool isReady) =>
        isReady ? "supply_clear" : "supply_blockers";

    public static bool IsActivePartStatus(string status) =>
        string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);

    public static bool IsActiveSupplierStatus(string status) =>
        string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);

    public static bool IsBlockedApprovalStatus(string approvalStatus) =>
        BlockedApprovalStatuses.Contains(approvalStatus);

    public static bool IsPendingApprovalStatus(string approvalStatus) =>
        PendingApprovalStatuses.Contains(approvalStatus);

    public static string? ResolveApprovalBlockerReasonCode(string approvalStatus)
    {
        if (string.Equals(approvalStatus, "blocked", StringComparison.OrdinalIgnoreCase))
        {
            return SupplyReadinessReasonCodes.SupplierApprovalBlocked;
        }

        if (string.Equals(approvalStatus, "restricted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(approvalStatus, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            return SupplyReadinessReasonCodes.SupplierApprovalRestricted;
        }

        if (IsPendingApprovalStatus(approvalStatus))
        {
            return SupplyReadinessReasonCodes.SupplierApprovalPending;
        }

        return null;
    }
}
