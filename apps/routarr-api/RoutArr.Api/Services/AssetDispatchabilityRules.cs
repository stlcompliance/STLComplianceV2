using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public static class AssetDispatchabilityRules
{
    public static string MergeOutcome(string? maintainArrReadinessStatus, bool assetFound)
    {
        if (!assetFound)
        {
            return AssetDispatchabilityOutcomes.Warn;
        }

        if (string.Equals(maintainArrReadinessStatus, "not_ready", StringComparison.OrdinalIgnoreCase))
        {
            return AssetDispatchabilityOutcomes.Block;
        }

        return AssetDispatchabilityOutcomes.Allow;
    }

    public static bool IsBlockingOutcome(string outcome) =>
        string.Equals(outcome, AssetDispatchabilityOutcomes.Block, StringComparison.OrdinalIgnoreCase);

    public static (string ReasonCode, string Message) BuildMergedReason(
        string outcome,
        AssetDispatchabilityMaintainArrSummary? maintainArr,
        bool assetFound)
    {
        if (string.Equals(outcome, AssetDispatchabilityOutcomes.Block, StringComparison.OrdinalIgnoreCase))
        {
            if (maintainArr is not null
                && string.Equals(maintainArr.ReadinessStatus, "not_ready", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    "maintainarr_not_ready",
                    maintainArr.PrimaryBlockerMessage ?? "Asset is not ready for dispatch.");
            }

            return ("asset_dispatchability_blocked", "Asset dispatchability check blocked assignment.");
        }

        if (string.Equals(outcome, AssetDispatchabilityOutcomes.Warn, StringComparison.OrdinalIgnoreCase))
        {
            if (!assetFound)
            {
                return (
                    "maintainarr_asset_not_found",
                    "Asset was not found in MaintainArr; dispatchability could not be confirmed.");
            }

            return ("asset_dispatchability_warn", "Asset dispatchability check returned warnings.");
        }

        return ("asset_dispatchability_clear", "Asset meets dispatch readiness requirements.");
    }

    public static DispatchAssignmentPreviewResponse ApplyDispatchability(
        DispatchAssignmentPreviewResponse preview,
        AssetDispatchabilityCheckResponse? dispatchability)
    {
        if (dispatchability is null)
        {
            return preview;
        }

        var summary = new DispatchAssignmentDispatchabilitySummary(
            dispatchability.Outcome,
            dispatchability.ReasonCode,
            dispatchability.Message,
            dispatchability.IsBlocking,
            dispatchability.MaintainArr);

        var hasBlocking = preview.HasBlockingConflicts || dispatchability.IsBlocking;

        return preview with
        {
            CanAssign = !hasBlocking,
            HasBlockingConflicts = hasBlocking,
            AssetDispatchability = summary,
        };
    }
}
