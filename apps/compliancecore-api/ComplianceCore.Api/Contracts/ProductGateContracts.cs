namespace ComplianceCore.Api.Contracts;

public sealed record ProductGateSubjectReference(
    string SubjectType,
    string SubjectReference,
    string? SourceProduct = null,
    string? DisplayLabel = null);

public sealed record ProductGateFactSnapshotReference(
    string FactKey,
    string SnapshotReference,
    DateTimeOffset CapturedAt,
    DateTimeOffset? ExpiresAt = null);

public sealed record ProductGateEvaluationRequest(
    Guid TenantId,
    string WorkflowKey,
    string ActionKey,
    string ActivityContextKey,
    IReadOnlyList<ProductGateSubjectReference> SubjectReferences,
    IReadOnlyDictionary<string, string>? RuleContext = null,
    IReadOnlyList<ProductGateFactSnapshotReference>? FactSnapshotReferences = null,
    bool EmitFindings = false);

public sealed record ProductGateCompatibilityRequest(
    Guid TenantId,
    string ActivityContextKey,
    IReadOnlyList<ProductGateSubjectReference> SubjectReferences,
    IReadOnlyDictionary<string, string>? RuleContext = null,
    IReadOnlyList<ProductGateFactSnapshotReference>? FactSnapshotReferences = null,
    bool EmitFindings = false,
    string? WorkflowKey = null);

public sealed record ProductGateAppliedRuleVersion(
    string RuleKey,
    string Label,
    string Result,
    string Message,
    int RulePackVersion,
    bool NonWaivable);

public sealed record ProductGateCitationReference(
    Guid CitationId,
    string CitationKey,
    string SourceReference,
    int VersionNumber);

public sealed record ProductGateEvidenceRequirement(
    Guid FactRequirementId,
    string RequirementKey,
    string FactKey,
    string Label,
    string Description,
    bool IsRequired,
    string? CitationKey);

public sealed record ProductGateRemediationHint(
    string Code,
    string Message,
    string? RuleKey,
    string? FactKey);

public sealed record ProductGateStaleFactReference(
    string FactKey,
    string SnapshotReference,
    DateTimeOffset CapturedAt,
    DateTimeOffset ExpiresAt);

public sealed record ProductGateEvaluationResponse(
    Guid TraceId,
    Guid TenantId,
    string WorkflowKey,
    string ActionKey,
    string ActivityContextKey,
    IReadOnlyList<ProductGateSubjectReference> SubjectReferences,
    Guid CheckResultId,
    Guid? RuleEvaluationRunId,
    string Outcome,
    string ReasonCode,
    string Message,
    IReadOnlyList<ProductGateAppliedRuleVersion> AppliedRuleVersions,
    IReadOnlyList<ProductGateCitationReference> CitationReferences,
    IReadOnlyList<string> MissingFacts,
    IReadOnlyList<ProductGateStaleFactReference> StaleFacts,
    IReadOnlyList<ProductGateEvidenceRequirement> EvidenceRequirements,
    IReadOnlyList<ProductGateRemediationHint> RemediationHints,
    Guid? AppliedWaiverId,
    string? AppliedWaiverKey,
    string? AuditExportPath,
    DateTimeOffset EvaluatedAt);
