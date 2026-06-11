using NexArr.Api.Contracts;

namespace NexArr.Api.Services;

public static class FieldCompanionDeniedReasonCatalog
{
    private static readonly Dictionary<string, string> PlainMessages = new(StringComparer.Ordinal)
    {
        [FieldCompanionFieldValidationReasonCodes.InvalidTaskKey] =
            "This task reference is not recognized. Scan or select a task from your field inbox.",
        [FieldCompanionFieldValidationReasonCodes.ProductMismatch] =
            "The product selected does not match this task.",
        [FieldCompanionFieldValidationReasonCodes.UnsupportedSubmissionKind] =
            "That type of field submission is not supported.",
        [FieldCompanionFieldValidationReasonCodes.NotEntitled] =
            "Your account is not entitled to work on tasks for this product.",
        [FieldCompanionFieldValidationReasonCodes.NotInInbox] =
            "This task is not in your field inbox. Refresh your inbox or ask a supervisor to reassign the work.",
        [FieldCompanionFieldValidationReasonCodes.EvidenceUnsupported] =
            "Evidence capture is not available for this task yet. Open the task in the owning product app.",
        [FieldCompanionFieldValidationReasonCodes.DvirUnsupported] =
            "DVIR submission is not available for this task. Open the trip in RoutArr to complete inspection paperwork.",
        [FieldCompanionFieldValidationReasonCodes.InspectionUnsupported] =
            "Inspection capture is not available for this task. Open the inspection in MaintainArr to continue.",
        [FieldCompanionFieldValidationReasonCodes.WorkOrderUnsupported] =
            "Work order updates are not available for this task. Open the work order in MaintainArr to continue.",
        [FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported] =
            "Receiving updates are not available for this task. Open the receipt in the owning product to continue.",
        [FieldCompanionFieldValidationReasonCodes.InboxUnavailable] =
            "We could not load the product inbox to verify this task. Try again when connectivity improves.",
        [FieldCompanionScanReasonCodes.InvalidPayload] =
            "The scan did not contain a recognizable field task.",
        [FieldCompanionScanReasonCodes.NotEntitled] =
            "You are not entitled to open tasks for this product.",
        [FieldCompanionScanReasonCodes.NotInInbox] =
            "This task is not in your field inbox.",
        ["auth.not_entitled"] =
            "Field Companion access requires a Field Companion entitlement or a field-product entitlement.",
        ["auth.unauthorized"] = "Sign in again to continue field work.",
        ["fieldcompanion.offline_actions.idempotency_required"] =
            "Each offline action needs a unique idempotency key before sync.",
        ["fieldcompanion.offline_actions.task_required"] =
            "Each offline action must include a task and product reference.",
        ["fieldcompanion.offline_actions.unsupported_kind"] =
            "Only field inbox acknowledgments can be queued offline right now.",
        ["launch.denied"] = "Product launch is not permitted.",
        ["tenant_suspended"] = "This tenant is suspended. Contact your administrator.",
        ["not_entitled"] = "Your account is not entitled to open this product.",
        ["entitlement_inactive"] = "This product entitlement is inactive for your tenant.",
        ["profile_missing"] = "Launch is not configured for this product yet.",
        ["product_url_missing"] = "This product API is not configured for field inbox aggregation.",
        ["upstream_unreachable"] = "Could not reach the product inbox. Try again when connectivity improves.",
    };

    public static string ToPlainMessage(string code, string? fallback = null) =>
        PlainMessages.TryGetValue(code, out var plain) ? plain : fallback ?? "This field action is not allowed.";

    public static string ForBlockedTask(string blockedReason) =>
        string.IsNullOrWhiteSpace(blockedReason)
            ? "This task is blocked until required work is completed."
            : blockedReason.Trim();
}
