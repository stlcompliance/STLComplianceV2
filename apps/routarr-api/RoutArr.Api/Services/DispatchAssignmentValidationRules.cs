using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class DispatchAssignmentValidationRules
{
    public static TripAssignmentValidation ValidateTripAssignable(Trip trip)
    {
        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            return new TripAssignmentValidation(
                false,
                "trip.not_assignable",
                $"Trip status '{trip.DispatchStatus}' does not allow assignment.");
        }

        return new TripAssignmentValidation(true, null, null);
    }

    public static DispatchAssignmentConflictSummary BuildConflictSummary(
        DispatchAssignmentPreviewResponse preview)
    {
        var eligibilityBlocking = preview.DriverEligibility?.IsBlocking == true;
        var eligibilityWarning =
            preview.DriverEligibility?.Outcome is "warn"
            && !eligibilityBlocking;
        var dispatchabilityBlocking = preview.AssetDispatchability?.IsBlocking == true;
        var dispatchabilityWarning =
            preview.AssetDispatchability?.Outcome is "warn"
            && !dispatchabilityBlocking;
        var workflowBlocking = preview.WorkflowGates?.IsBlocking == true;
        var workflowWarning =
            preview.WorkflowGates?.Outcome is "warn"
            && !workflowBlocking;
        var externalReasonCodes = CollectExternalReasonCodes(preview);
        var hasMissingExternalData = externalReasonCodes.Any(IsMissingExternalDataReasonCode);
        var hasStaleExternalData = externalReasonCodes.Any(IsStaleExternalDataReasonCode);

        return new DispatchAssignmentConflictSummary(
            preview.BlockingDriverAvailability.Count,
            preview.BlockingEquipmentAvailability.Count,
            preview.OverlappingTrips.Count,
            eligibilityBlocking,
            eligibilityWarning,
            dispatchabilityBlocking,
            dispatchabilityWarning,
            workflowBlocking,
            workflowWarning,
            hasMissingExternalData,
            hasStaleExternalData);
    }

    public static IReadOnlyList<string> BuildValidationMessages(
        TripAssignmentValidation tripValidation,
        DispatchAssignmentPreviewResponse preview)
    {
        var messages = new List<string>();
        if (!tripValidation.IsValid && tripValidation.Message is not null)
        {
            messages.Add(tripValidation.Message);
        }

        if (preview.BlockingDriverAvailability.Count > 0)
        {
            messages.Add($"{preview.BlockingDriverAvailability.Count} driver availability conflict(s).");
        }

        if (preview.BlockingEquipmentAvailability.Count > 0)
        {
            messages.Add($"{preview.BlockingEquipmentAvailability.Count} equipment availability conflict(s).");
        }

        if (preview.OverlappingTrips.Count > 0)
        {
            messages.Add($"{preview.OverlappingTrips.Count} overlapping trip(s).");
        }

        if (preview.DriverEligibility?.IsBlocking == true)
        {
            messages.Add(preview.DriverEligibility.Message);
        }
        else if (preview.DriverEligibility?.Outcome is "warn")
        {
            messages.Add($"Eligibility warning: {preview.DriverEligibility.Message}");
        }

        if (preview.AssetDispatchability?.IsBlocking == true)
        {
            messages.Add(preview.AssetDispatchability.Message);
        }
        else if (preview.AssetDispatchability?.Outcome is "warn")
        {
            messages.Add($"Dispatchability warning: {preview.AssetDispatchability.Message}");
        }

        if (preview.WorkflowGates?.IsBlocking == true)
        {
            messages.Add(preview.WorkflowGates.Message);
        }
        else if (preview.WorkflowGates?.Outcome is "warn")
        {
            messages.Add($"Workflow gate warning: {preview.WorkflowGates.Message}");
        }

        var externalReasonCodes = CollectExternalReasonCodes(preview);
        if (externalReasonCodes.Any(IsMissingExternalDataReasonCode))
        {
            messages.Add("External data is missing or unavailable; dispatch checks may be incomplete.");
        }

        if (externalReasonCodes.Any(IsStaleExternalDataReasonCode))
        {
            messages.Add("External data may be stale; validate readiness before dispatch release.");
        }

        return messages;
    }

    public static string? ResolvePrimaryBlockCode(
        TripAssignmentValidation tripValidation,
        DispatchAssignmentPreviewResponse preview)
    {
        if (!tripValidation.IsValid)
        {
            return tripValidation.BlockCode;
        }

        if (preview.BlockingDriverAvailability.Count > 0
            || preview.BlockingEquipmentAvailability.Count > 0)
        {
            return "dispatch.assignment_availability_blocked";
        }

        if (preview.OverlappingTrips.Count > 0)
        {
            return "dispatch.assignment_overlap_blocked";
        }

        if (preview.DriverEligibility?.IsBlocking == true)
        {
            return preview.DriverEligibility.ReasonCode;
        }

        if (preview.AssetDispatchability?.IsBlocking == true)
        {
            return preview.AssetDispatchability.ReasonCode;
        }

        if (preview.WorkflowGates?.IsBlocking == true)
        {
            return preview.WorkflowGates.ReasonCode;
        }

        return null;
    }

    public static DispatchAssignmentPreviewResponse ApplyValidation(
        Trip trip,
        DispatchAssignmentPreviewResponse preview)
    {
        var tripValidation = ValidateTripAssignable(trip);
        var summary = BuildConflictSummary(preview);
        var messages = BuildValidationMessages(tripValidation, preview);
        var primaryBlockCode = ResolvePrimaryBlockCode(tripValidation, preview);
        var canAssign = preview.CanAssign && tripValidation.IsValid;
        var hasBlocking = preview.HasBlockingConflicts || !tripValidation.IsValid;

        return preview with
        {
            CanAssign = canAssign,
            HasBlockingConflicts = hasBlocking,
            ConflictSummary = summary,
            ValidationMessages = messages,
            PrimaryBlockCode = primaryBlockCode,
        };
    }

    private static IReadOnlyList<string> CollectExternalReasonCodes(DispatchAssignmentPreviewResponse preview)
    {
        var codes = new List<string>();

        if (!string.IsNullOrWhiteSpace(preview.DriverEligibility?.ReasonCode))
        {
            codes.Add(preview.DriverEligibility!.ReasonCode);
        }

        if (!string.IsNullOrWhiteSpace(preview.AssetDispatchability?.ReasonCode))
        {
            codes.Add(preview.AssetDispatchability!.ReasonCode);
        }

        if (!string.IsNullOrWhiteSpace(preview.WorkflowGates?.ReasonCode))
        {
            codes.Add(preview.WorkflowGates!.ReasonCode);
        }

        if (preview.WorkflowGates?.Gates is not null)
        {
            codes.AddRange(preview.WorkflowGates.Gates
                .Where(g => !string.IsNullOrWhiteSpace(g.ReasonCode))
                .Select(g => g.ReasonCode));
        }

        return codes;
    }

    private static bool IsMissingExternalDataReasonCode(string reasonCode)
    {
        var code = reasonCode.ToLowerInvariant();
        return code.Contains("unavailable", StringComparison.Ordinal)
            || code.Contains("not_found", StringComparison.Ordinal)
            || code.Contains("missing", StringComparison.Ordinal)
            || code.Contains("required", StringComparison.Ordinal);
    }

    private static bool IsStaleExternalDataReasonCode(string reasonCode) =>
        reasonCode.Contains("stale", StringComparison.OrdinalIgnoreCase);
}

public sealed record TripAssignmentValidation(bool IsValid, string? BlockCode, string? Message);
