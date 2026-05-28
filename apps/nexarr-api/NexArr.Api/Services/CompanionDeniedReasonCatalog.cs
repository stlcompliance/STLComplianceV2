using NexArr.Api.Contracts;

namespace NexArr.Api.Services;

public static class CompanionDeniedReasonCatalog
{
    private static readonly Dictionary<string, string> PlainMessages = new(StringComparer.Ordinal)
    {
        [CompanionFieldValidationReasonCodes.InvalidTaskKey] =
            "This task reference is not recognized. Scan or select a task from your field inbox.",
        [CompanionFieldValidationReasonCodes.ProductMismatch] =
            "The product selected does not match this task.",
        [CompanionFieldValidationReasonCodes.UnsupportedSubmissionKind] =
            "That type of field submission is not supported.",
        [CompanionFieldValidationReasonCodes.NotEntitled] =
            "Your account is not entitled to work on tasks for this product.",
        [CompanionFieldValidationReasonCodes.NotInInbox] =
            "This task is not in your field inbox. Refresh your inbox or ask a supervisor to reassign the work.",
        [CompanionFieldValidationReasonCodes.EvidenceUnsupported] =
            "Evidence capture is not available for this task yet. Open the task in the owning product app.",
        [CompanionFieldValidationReasonCodes.InboxUnavailable] =
            "We could not load the product inbox to verify this task. Try again when connectivity improves.",
        [CompanionScanReasonCodes.InvalidPayload] =
            "The scan did not contain a recognizable field task.",
        [CompanionScanReasonCodes.NotEntitled] =
            "You are not entitled to open tasks for this product.",
        [CompanionScanReasonCodes.NotInInbox] =
            "This task is not in your field inbox.",
        ["auth.not_entitled"] =
            "Companion access requires companion or field-product entitlement.",
        ["auth.unauthorized"] = "Sign in again to continue field work.",
        ["companion.offline_actions.idempotency_required"] =
            "Each offline action needs a unique idempotency key before sync.",
        ["companion.offline_actions.task_required"] =
            "Each offline action must include a task and product reference.",
        ["companion.offline_actions.unsupported_kind"] =
            "Only field inbox acknowledgments can be queued offline right now.",
    };

    public static string ToPlainMessage(string code, string? fallback = null) =>
        PlainMessages.TryGetValue(code, out var plain) ? plain : fallback ?? "This field action is not allowed.";

    public static string ForBlockedTask(string blockedReason) =>
        string.IsNullOrWhiteSpace(blockedReason)
            ? "This task is blocked until required work is completed."
            : blockedReason.Trim();
}
