namespace NexArr.Api.Contracts;

public sealed record ValidateFieldCompanionFieldTaskRequest(
    string TaskKey,
    string SubmissionKind,
    string? ProductKey = null);

public sealed record ValidateFieldCompanionFieldTaskResponse(
    bool Allowed,
    string? ReasonCode,
    string? ReasonMessage,
    string TaskKey,
    string ProductKey,
    string? Title,
    string? BlockedReason);

public static class FieldCompanionFieldValidationReasonCodes
{
    public const string InvalidTaskKey = "fieldcompanion.field_task.invalid_key";
    public const string ProductMismatch = "fieldcompanion.field_task.product_mismatch";
    public const string UnsupportedSubmissionKind = "fieldcompanion.field_task.unsupported_submission_kind";
    public const string AccessUnavailable = "fieldcompanion.field_task.not_available";
    public const string NotInInbox = "fieldcompanion.field_task.not_in_inbox";
    public const string EvidenceUnsupported = "fieldcompanion.field_evidence.unsupported_task";
    public const string DvirUnsupported = "fieldcompanion.field_dvir.unsupported_task";
    public const string InspectionUnsupported = "fieldcompanion.field_inspection.unsupported_task";
    public const string WorkOrderUnsupported = "fieldcompanion.field_work_order.unsupported_task";
    public const string ReceivingUnsupported = "fieldcompanion.field_receiving.unsupported_task";
    public const string InboxUnavailable = "fieldcompanion.field_task.inbox_unavailable";
}
