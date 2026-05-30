namespace ComplianceCore.Api.Contracts;

public sealed record AuditRequirementMatrixResponse(
    string GroupBy,
    string Key,
    IReadOnlyList<FactRequirementResponse> Requirements);

public sealed record EvidenceReferenceCreateRequest(
    Guid TenantId,
    string EvidenceId,
    string FactKey,
    string SourceProduct,
    string SourceEntity,
    string SourceRecordId,
    string SourceField,
    string DocumentType,
    string? DocumentUrl,
    string? StorageKey,
    string FileHash,
    DateTimeOffset CapturedAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    Guid? CreatedByPersonId,
    Guid? ReviewedByPersonId,
    string ReviewStatus,
    string Notes);

public sealed record EvidenceReferenceResponse(
    Guid EvidenceReferenceId,
    string EvidenceId,
    Guid TenantId,
    string FactKey,
    string SourceProduct,
    string SourceEntity,
    string SourceRecordId,
    string SourceField,
    string DocumentType,
    string DocumentUrl,
    string StorageKey,
    string FileHash,
    DateTimeOffset CapturedAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    Guid? CreatedByPersonId,
    Guid? ReviewedByPersonId,
    string ReviewStatus,
    string Notes);

public sealed record FactAssertionCreateRequest(
    Guid TenantId,
    string FactKey,
    string SubjectKind,
    string SubjectId,
    string Value,
    string ValueType,
    string SourceProduct,
    string SourceRecordId,
    string? EvidenceId,
    DateTimeOffset AssertedAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt);

public sealed record FactAssertionResponse(
    Guid FactAssertionId,
    Guid TenantId,
    string FactKey,
    string SubjectKind,
    string SubjectId,
    string Value,
    string ValueType,
    string SourceProduct,
    string SourceRecordId,
    string? EvidenceId,
    DateTimeOffset AssertedAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt);

public sealed record AuditRequirementEvaluationRequest(
    string PackKey,
    string SubjectKind,
    string SubjectId,
    IReadOnlyDictionary<string, string>? OverrideReasons = null,
    Guid? OverridePersonId = null);

public sealed record AuditRequirementEvaluationResponse(
    string PackKey,
    string SubjectKind,
    string SubjectId,
    string OverallResult,
    IReadOnlyList<AuditTraceResponse> Traces,
    DateTimeOffset EvaluatedAt);

public sealed record AuditTraceResponse(
    Guid AuditTraceRowId,
    string AuditTraceId,
    string PackKey,
    string FactKey,
    string CitationKey,
    string SubjectKind,
    string SubjectId,
    string EvaluatedValue,
    string ExpectedValue,
    string Operator,
    string Result,
    string FailureSeverity,
    bool AutomaticFailureFlag,
    bool OverrideUsed,
    Guid? OverridePersonId,
    string OverrideReason,
    bool RemediationRequired,
    DateTimeOffset EvaluatedAt);
