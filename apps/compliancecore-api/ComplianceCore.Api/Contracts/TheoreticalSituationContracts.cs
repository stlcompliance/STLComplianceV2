namespace ComplianceCore.Api.Contracts;

public sealed record CreateTheoreticalSituationRequest(
    string SituationKind,
    string? Title = null);

public sealed record UpdateTheoreticalSituationRequest(
    string? Title = null,
    string? SituationKind = null,
    string? Status = null);

public sealed record TheoreticalSituationResponse(
    Guid SituationId,
    Guid TenantId,
    Guid CreatedByPersonId,
    string Title,
    string SituationKind,
    string Status,
    string EvaluationMode,
    bool SavedAsTemplate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TheoreticalSituationContextResponse> Context,
    IReadOnlyList<TheoreticalSituationFactResponse> Facts,
    IReadOnlyList<TheoreticalSituationIncidentResponse> Incidents,
    TheoreticalSituationEvaluationResponse? LatestEvaluation);

public sealed record TheoreticalSituationListItemResponse(
    Guid SituationId,
    string Title,
    string SituationKind,
    string Status,
    bool SavedAsTemplate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? LatestResult);

public sealed record TheoreticalOptionResponse(
    string Key,
    string Label,
    string Description,
    string Category,
    bool EdgeCase = false);

public sealed record TheoreticalContextFieldResponse(
    string ContextKey,
    string Label,
    string ControlType,
    string ControlledVocabularyType,
    bool Required,
    IReadOnlyList<string> SituationKinds,
    IReadOnlyList<TheoreticalOptionResponse> Values);

public sealed record TheoreticalSituationContextRequest(
    IReadOnlyList<TheoreticalSituationContextValueRequest> Values);

public sealed record TheoreticalSituationContextValueRequest(
    string ContextKey,
    string ContextValueKey);

public sealed record TheoreticalSituationContextResponse(
    Guid ContextId,
    string ContextKey,
    string ContextLabel,
    string ContextValueKey,
    string ContextValueLabel,
    string ControlledVocabularyType,
    decimal Confidence,
    DateTimeOffset CreatedAt);

public sealed record TheoreticalSituationFactRequest(
    IReadOnlyList<TheoreticalSituationFactValueRequest> Facts);

public sealed record TheoreticalSituationFactValueRequest(
    string FactKey,
    string SimulatedState,
    string? RequirementKey = null,
    string? CitationKey = null,
    string? PackKey = null,
    string? SimulatedValue = null,
    string? ValueType = null,
    string? EvidenceOptionKey = null,
    string? EvidenceKind = null,
    string? TargetKind = null);

public sealed record TheoreticalSituationFactResponse(
    Guid SituationFactId,
    string FactKey,
    string RequirementKey,
    string CitationKey,
    string PackKey,
    string SimulatedValue,
    string ValueType,
    string SimulatedState,
    string EvidenceOptionKey,
    string EvidenceKind,
    string TargetKind,
    bool Active,
    DateTimeOffset CreatedAt);

public sealed record TheoreticalSituationIncidentRequest(
    IReadOnlyList<TheoreticalSituationIncidentValueRequest> Incidents);

public sealed record TheoreticalSituationIncidentValueRequest(
    string IncidentTypeKey,
    string SeverityKey,
    string InvolvedSubjectKind,
    string InvolvedSubjectState,
    string TriggerKey,
    string TriggerValue,
    string ReportabilityState,
    string RemediationState);

public sealed record TheoreticalSituationIncidentResponse(
    Guid SituationIncidentId,
    string IncidentTypeKey,
    string SeverityKey,
    string InvolvedSubjectKind,
    string InvolvedSubjectState,
    string TriggerKey,
    string TriggerValue,
    string ReportabilityState,
    string RemediationState,
    DateTimeOffset CreatedAt);

public sealed record TheoreticalApplicabilityResultResponse(
    Guid ApplicabilityResultId,
    string ProgramKey,
    string PackKey,
    string CitationKey,
    decimal ApplicabilityScore,
    string ApplicabilityBand,
    IReadOnlyList<string> MatchReasons,
    IReadOnlyList<string> MissingContext,
    IReadOnlyList<string> ExclusionReasons,
    bool EdgeCase,
    string EdgeCaseReason,
    int UserVisiblePriority,
    DateTimeOffset CreatedAt);

public sealed record TheoreticalNextContextResponse(
    IReadOnlyList<TheoreticalContextFieldResponse> Questions,
    bool ReadyForApplicability,
    string Summary);

public sealed record TheoreticalEvidenceOptionResponse(
    Guid EvidenceOptionId,
    string EvidenceOptionKey,
    string EvidenceOptionLabel,
    string LogicType,
    string RequirementKey,
    string FactKey,
    string EvidenceKind,
    string TargetKind,
    string SourceProduct,
    string SourceEntity,
    bool Required);

public sealed record TheoreticalEvaluateRequest(
    bool IncludePossible = false);

public sealed record TheoreticalSituationEvaluationResponse(
    Guid EvaluationId,
    Guid SituationId,
    DateTimeOffset EvaluatedAt,
    Guid EvaluatedByPersonId,
    string Result,
    string Summary,
    IReadOnlyList<string> PrimaryPrograms,
    IReadOnlyList<string> LikelyPrograms,
    IReadOnlyList<string> EdgeCases,
    int PassCount,
    int FailCount,
    int WarningCount,
    int BlockedCount,
    int NotApplicableCount,
    int UnknownCount,
    int OverrideAvailableCount,
    int OverrideBlockedCount,
    IReadOnlyList<TheoreticalSituationEvaluationDetailResponse> Details);

public sealed record TheoreticalSituationEvaluationDetailResponse(
    Guid DetailId,
    string RequirementKey,
    string FactKey,
    string CitationKey,
    string PackKey,
    string AuditQuestion,
    string SimulatedState,
    string ExpectedValue,
    string ActualValue,
    string Operator,
    string Result,
    string FailureSeverity,
    bool AutomaticFailureFlag,
    bool OverrideAllowed,
    string OverridePermission,
    bool RemediationRequired,
    string NormalRuleResult,
    string ExceptionExemptionKey,
    string ExceptionExemptionType,
    string ExceptionExemptionLabel,
    bool ExceptionExemptionConsidered,
    bool ExceptionExemptionApplies,
    bool ExceptionExemptionProofRequired,
    bool ExceptionExemptionProofValid,
    string ResultBeforeException,
    string ResultAfterException,
    string FinalComplianceResult,
    string Explanation,
    string SuggestedNextAction,
    int VisiblePriority);
