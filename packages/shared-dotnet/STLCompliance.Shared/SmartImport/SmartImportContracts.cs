using System.Text.Json;

namespace STLCompliance.Shared.SmartImport;

public static class SmartImportStatuses
{
    public const string Uploaded = "uploaded";
    public const string Processing = "processing";
    public const string ReviewRequired = "review_required";
    public const string ReadyToCommit = "ready_to_commit";
    public const string Committing = "committing";
    public const string Committed = "committed";
    public const string PartiallyCommitted = "partially_committed";
    public const string Rejected = "rejected";
    public const string Failed = "failed";
}

public static class SmartImportReviewReasons
{
    public const string UnsupportedProductApi = "unsupported_product_api";
    public const string PersonCreateOrLink = "person_create_or_link";
    public const string TrainingOrCertificationRecord = "training_or_certification_record";
    public const string ComplianceCoreImport = "compliancecore_import";
    public const string AssetUpdate = "asset_update";
    public const string Overwrite = "overwrite";
    public const string LowConfidenceComplianceField = "low_confidence_compliance_field";
    public const string MoneyAmount = "money_amount";
    public const string DuplicateMatch = "duplicate_match";
    public const string UnresolvedLocation = "unresolved_location";
    public const string ScanOrHandwriting = "scan_or_handwriting";
    public const string RegulatoryRetention = "regulatory_retention";
    public const string ConflictingProduct = "conflicting_product";
    public const string DuplicateRecord = "duplicate_record";
    public const string CustomArrFallback = "customarr_fallback";
    public const string HumanConfirmationRequired = "human_confirmation_required";
}

public static class SmartImportConfidencePolicy
{
    public const string AutofillPreviewed = "autofill_previewed";
    public const string Preselected = "preselected";
    public const string ReviewRequired = "review_required";
    public const string WeakNotPreselected = "weak_not_preselected";
    public const string NoteOnly = "note_only";

    public static string GetDisposition(decimal confidence)
    {
        if (confidence >= 95m)
        {
            return AutofillPreviewed;
        }

        if (confidence >= 85m)
        {
            return Preselected;
        }

        if (confidence >= 70m)
        {
            return ReviewRequired;
        }

        if (confidence >= 50m)
        {
            return WeakNotPreselected;
        }

        return NoteOnly;
    }

    public static bool RequiresReview(decimal confidence) => confidence < 85m;
}

public sealed record SmartImportUploadResponse(
    Guid BatchId,
    Guid FileId,
    string Status,
    string DestinationProduct,
    string? RecordArrRecordId,
    string? RecordArrFileId,
    string Message);

public sealed record SmartImportBatchSummary(
    Guid BatchId,
    Guid TenantId,
    Guid ActorPersonId,
    string Status,
    string DestinationProductHint,
    string SourceLabel,
    int FileCount,
    int ProposedRecordCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record SmartImportBatchDetail(
    SmartImportBatchSummary Batch,
    IReadOnlyList<SmartImportFileSummary> Files,
    IReadOnlyList<SmartImportClassificationSummary> Classifications,
    IReadOnlyList<SmartImportProposedRecordSummary> ProposedRecords,
    IReadOnlyList<SmartImportCommitPlanSummary> CommitPlans,
    IReadOnlyList<SmartImportAuditEventSummary> AuditEvents);

public sealed record SmartImportFileSummary(
    Guid FileId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string? RecordArrRecordId,
    string? RecordArrFileId,
    string Status);

public sealed record SmartImportClassificationSummary(
    Guid ClassificationId,
    string DestinationProduct,
    string EntityType,
    decimal Confidence,
    bool RequiresReview,
    IReadOnlyList<string> ReviewReasons,
    string? Notes);

public sealed record SmartImportProposedRecordSummary(
    Guid ProposedRecordId,
    string DestinationProduct,
    string EntityType,
    string Operation,
    decimal Confidence,
    string ReviewStatus,
    bool RequiresReview,
    IReadOnlyList<string> ReviewReasons,
    JsonElement ProposedPayload);

public sealed record SmartImportReviewDecisionRequest(
    Guid ProposedRecordId,
    string Decision,
    string? Notes,
    JsonElement? CorrectedPayload);

public sealed record SmartImportCommitPlanSummary(
    Guid CommitPlanId,
    string Status,
    int StepCount,
    int CompletedStepCount,
    int FailedStepCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovedAt);

public sealed record SmartImportAuditEventSummary(
    Guid AuditEventId,
    string EventType,
    string ActorType,
    Guid? ActorPersonId,
    string Result,
    string? ReasonCode,
    DateTimeOffset OccurredAt);

public sealed record SmartImportDestinationValidateRequest(
    Guid TenantId,
    Guid ActorPersonId,
    Guid ImportBatchId,
    Guid ProposedRecordId,
    string DestinationProduct,
    string EntityType,
    string Operation,
    JsonElement ProposedPayload,
    string? RecordArrSourceRecordId,
    string IdempotencyKey,
    string ReviewStatus,
    IReadOnlyList<Guid> SourceFieldIds);

public sealed record SmartImportDestinationValidateResponse(
    bool Valid,
    IReadOnlyList<string> RequiredPermissions,
    IReadOnlyList<string> MissingPermissions,
    IReadOnlyList<SmartImportFieldValidationResult> FieldResults,
    IReadOnlyList<SmartImportMatchCandidateResponse> MatchCandidates,
    IReadOnlyList<string> RequiredReviewReasons,
    JsonElement? DeterministicPayload,
    IReadOnlyList<string> Warnings);

public sealed record SmartImportDestinationCommitRequest(
    Guid TenantId,
    Guid ActorPersonId,
    Guid ApprovedByPersonId,
    Guid ImportBatchId,
    Guid CommitPlanId,
    Guid CommitStepId,
    string DestinationProduct,
    string EntityType,
    string Operation,
    JsonElement DeterministicPayload,
    string? RecordArrSourceRecordId,
    string IdempotencyKey);

public sealed record SmartImportDestinationCommitResponse(
    string Status,
    string? ResultEntityId,
    string? DisplayName,
    IReadOnlyList<SmartImportDestinationLink> Links,
    bool Retryable,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record SmartImportFieldValidationResult(
    string FieldKey,
    bool Valid,
    string? NormalizedValue,
    decimal? Confidence,
    IReadOnlyList<string> ReviewReasons,
    string? Message);

public sealed record SmartImportMatchCandidateResponse(
    string SourceProduct,
    string EntityType,
    string EntityId,
    string DisplayName,
    decimal Confidence,
    IReadOnlyList<string> MatchReasons);

public sealed record SmartImportDestinationLink(
    string Label,
    string Url,
    string SourceProduct);
