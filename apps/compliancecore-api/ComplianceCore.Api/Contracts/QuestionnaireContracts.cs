namespace ComplianceCore.Api.Contracts;

public sealed record QuestionnaireResolveRequest(
    Guid TenantId,
    string ProductKey,
    string WorkflowKey,
    string SubjectType,
    string? SubjectId = null,
    string? SubjectLabel = null,
    string? SourceRecordId = null,
    string? SourceEntity = null,
    IReadOnlyDictionary<string, string>? KnownFacts = null,
    IReadOnlyDictionary<string, string>? SourceRecordContext = null,
    bool PersistRun = true);

public sealed record QuestionnaireSubmitRequest(
    IReadOnlyList<QuestionnaireAnswerRequest> Answers,
    IReadOnlyDictionary<string, string>? SourceRecordContext = null);

public sealed record QuestionnaireAnswerRequest(
    string QuestionKey,
    string? SelectedOptionKey = null,
    string? AnswerText = null,
    string? DocumentUrl = null,
    string? StorageKey = null,
    string? FileName = null,
    string? FileHash = null,
    string? EvidenceId = null,
    DateTimeOffset? EffectiveAt = null);

public sealed record QuestionnaireResolutionResponse(
    QuestionnaireRunResponse Run,
    IReadOnlyList<QuestionnaireQuestionResponse> Questions,
    QuestionnaireTenantProfileResponse TenantProfile,
    QuestionnaireResultSummaryResponse Summary);

public sealed record QuestionnaireSubmissionResponse(
    QuestionnaireRunResponse Run,
    QuestionnaireTenantProfileResponse TenantProfile,
    QuestionnaireResultSummaryResponse Summary,
    IReadOnlyList<QuestionnaireAnswerResponse> Answers,
    IReadOnlyList<QuestionnaireFactResponse> CreatedFacts);

public sealed record QuestionnaireRunResponse(
    Guid QuestionnaireRunId,
    string ProductKey,
    string WorkflowKey,
    string SubjectType,
    string SubjectId,
    string SourceRecordId,
    string SourceEntity,
    string Status,
    string TemplateKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? SubmittedAt);

public sealed record QuestionnaireQuestionResponse(
    string QuestionKey,
    string SectionKey,
    string SectionLabel,
    string Prompt,
    string? HelpText,
    string? WhyItMatters,
    string AnswerKind,
    string FactKey,
    string FactValueType,
    bool Required,
    int Priority,
    string? DefaultOptionKey,
    IReadOnlyList<QuestionnaireAnswerOptionResponse> Options,
    IReadOnlyList<string> ApplicableAreas,
    IReadOnlyList<string> RecommendedNextActions);

public sealed record QuestionnaireAnswerOptionResponse(
    string Key,
    string Label,
    string Description,
    string AnswerKind,
    bool IsDefault = false);

public sealed record QuestionnaireAnswerResponse(
    Guid QuestionnaireAnswerId,
    string QuestionKey,
    string SelectedOptionKey,
    string AnswerText,
    string DocumentUrl,
    string StorageKey,
    string FileName,
    string FileHash,
    string NormalizedFactKey,
    string NormalizedFactValue,
    string NormalizedFactValueType,
    string ReviewStatus,
    decimal Confidence,
    DateTimeOffset EffectiveAt,
    Guid? EvidenceReferenceId,
    string? EvidenceId);

public sealed record QuestionnaireFactResponse(
    Guid FactAssertionId,
    string FactKey,
    string SubjectKind,
    string SubjectId,
    string Value,
    string ValueType,
    string SourceProduct,
    string SourceRecordId,
    string ReviewStatus,
    decimal Confidence,
    DateTimeOffset AssertedAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt);

public sealed record QuestionnaireTenantProfileResponse(
    string BusinessProfile,
    IReadOnlyList<string> TransportationExposure,
    IReadOnlyList<string> WorkforceExposure,
    IReadOnlyList<string> LocationExposure,
    IReadOnlyList<string> MaterialHazmatExposure,
    string RecordDocumentMaturity,
    IReadOnlyList<string> LikelyRulePacks,
    IReadOnlyList<string> InitialAssumptions,
    IReadOnlyList<string> SetupChecklist);

public sealed record QuestionnaireResultSummaryResponse(
    string Summary,
    IReadOnlyList<string> LikelyApplicableAreas,
    IReadOnlyList<string> MissingFacts,
    IReadOnlyList<string> RecommendedNextActions,
    IReadOnlyList<QuestionnaireExceptionResponse> GeneratedExceptions,
    IReadOnlyList<QuestionnaireFollowUpResponse> FollowUps,
    bool RequiresMoreFacts,
    string RiskGateStatus);

public sealed record QuestionnaireExceptionResponse(
    string ExceptionKey,
    string Label,
    string Reason,
    string Severity);

public sealed record QuestionnaireFollowUpResponse(
    string FollowUpKey,
    string Prompt,
    string Reason,
    string TriggerFactKey,
    string Priority);
