using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public static class DriverEligibilityRules
{
    public static string MergeOutcome(string? trainArrOutcome, string? staffArrReadinessStatus)
    {
        if (string.Equals(staffArrReadinessStatus, "not_ready", StringComparison.OrdinalIgnoreCase))
        {
            return DriverEligibilityOutcomes.Block;
        }

        if (string.Equals(trainArrOutcome, DriverEligibilityOutcomes.Block, StringComparison.OrdinalIgnoreCase))
        {
            return DriverEligibilityOutcomes.Block;
        }

        if (string.Equals(trainArrOutcome, DriverEligibilityOutcomes.Warn, StringComparison.OrdinalIgnoreCase))
        {
            return DriverEligibilityOutcomes.Warn;
        }

        return DriverEligibilityOutcomes.Allow;
    }

    public static bool IsBlockingOutcome(string outcome) =>
        string.Equals(outcome, DriverEligibilityOutcomes.Block, StringComparison.OrdinalIgnoreCase);

    public static (string ReasonCode, string Message) BuildMergedReason(
        string outcome,
        DriverEligibilityTrainArrSummary? trainArr,
        DriverEligibilityStaffArrSummary? staffArr)
    {
        if (string.Equals(outcome, DriverEligibilityOutcomes.Block, StringComparison.OrdinalIgnoreCase))
        {
            if (staffArr is not null
                && string.Equals(staffArr.ReadinessStatus, "not_ready", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    "staffarr_not_ready",
                    staffArr.PrimaryBlockerMessage ?? "Driver is not ready for dispatch.");
            }

            if (trainArr is not null
                && string.Equals(trainArr.Outcome, DriverEligibilityOutcomes.Block, StringComparison.OrdinalIgnoreCase))
            {
                return (trainArr.ReasonCode, trainArr.Message);
            }

            return ("driver_eligibility_blocked", "Driver eligibility check blocked assignment.");
        }

        if (string.Equals(outcome, DriverEligibilityOutcomes.Warn, StringComparison.OrdinalIgnoreCase))
        {
            if (trainArr is not null)
            {
                return (trainArr.ReasonCode, trainArr.Message);
            }

            return ("driver_eligibility_warn", "Driver eligibility check returned warnings.");
        }

        return ("driver_eligibility_clear", "Driver meets dispatch eligibility requirements.");
    }

    public static DispatchAssignmentPreviewResponse ApplyEligibility(
        DispatchAssignmentPreviewResponse preview,
        DriverEligibilityCheckResponse? eligibility)
    {
        if (eligibility is null)
        {
            return preview;
        }

        var summary = new DispatchAssignmentEligibilitySummary(
            eligibility.Outcome,
            eligibility.ReasonCode,
            eligibility.Message,
            eligibility.IsBlocking,
            eligibility.TrainArr,
            eligibility.StaffArr);

        var hasBlocking = preview.HasBlockingConflicts || eligibility.IsBlocking;

        return preview with
        {
            CanAssign = !hasBlocking,
            HasBlockingConflicts = hasBlocking,
            DriverEligibility = summary,
        };
    }
}
