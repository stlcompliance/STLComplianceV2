namespace NexArr.Api.Contracts;

public sealed record ValidateCompanionFieldTaskRequest(
    string TaskKey,
    string SubmissionKind,
    string? ProductKey = null);

public sealed record ValidateCompanionFieldTaskResponse(
    bool Allowed,
    string? ReasonCode,
    string? ReasonMessage,
    string TaskKey,
    string ProductKey,
    string? Title,
    string? BlockedReason);

public static class CompanionFieldValidationReasonCodes
{
    public const string InvalidTaskKey = "companion.field_task.invalid_key";
    public const string ProductMismatch = "companion.field_task.product_mismatch";
    public const string UnsupportedSubmissionKind = "companion.field_task.unsupported_submission_kind";
    public const string NotEntitled = "companion.field_task.not_entitled";
    public const string NotInInbox = "companion.field_task.not_in_inbox";
    public const string EvidenceUnsupported = "companion.field_evidence.unsupported_task";
    public const string DvirUnsupported = "companion.field_dvir.unsupported_task";
    public const string InspectionUnsupported = "companion.field_inspection.unsupported_task";
    public const string WorkOrderUnsupported = "companion.field_work_order.unsupported_task";
    public const string ReceivingUnsupported = "companion.field_receiving.unsupported_task";
    public const string InboxUnavailable = "companion.field_task.inbox_unavailable";
}
