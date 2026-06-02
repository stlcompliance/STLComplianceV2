namespace TrainArr.Api.Contracts;

public sealed record CreateQualificationCheckRequest(
    Guid StaffarrPersonId,
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context,
    DateTimeOffset? EffectiveAt = null,
    Guid? TrainingDefinitionId = null,
    Guid? TrainingProgramId = null);

public sealed record QualificationCheckResponse(
    Guid CheckId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string Outcome,
    string ReasonCode,
    string Message,
    QualificationLocalStateResponse? LocalQualification,
    ComplianceCoreCheckSummaryResponse? ComplianceCore,
    IReadOnlyList<QualificationDependencyFactResponse>? DependencyFacts = null,
    QualificationCheckAuditSnapshotResponse? AuditSnapshot = null,
    QualificationAuthorizationGuidanceResponse? AuthorizationGuidance = null,
    QualificationCatalogSnapshotResponse? QualificationCatalog = null);

public sealed record QualificationCatalogSnapshotResponse(
    string SourceProduct,
    string SourceEntity,
    Guid? SourceId,
    string QualificationKey,
    string LabelSnapshot,
    string StatusSnapshot,
    DateTimeOffset LastVerifiedAt,
    DateTimeOffset? LastSyncedAt);

public sealed record QualificationAuthorizationGuidanceResponse(
    string BlockReason,
    string MissingQualification,
    Guid? RequiredTrainingDefinitionId,
    string? RequiredTrainingDefinitionName,
    Guid? RequiredTrainingProgramId,
    string? RequiredTrainingProgramName,
    string PersonAssignmentStatus,
    Guid? TrainingAssignmentId,
    DateTimeOffset? AssignmentDueAt,
    string NextAction,
    string SupervisorAction,
    string EstimatedPathToQualification);

public sealed record QualificationDependencyFactResponse(
    string FactKey,
    string Status,
    string Message);

public sealed record QualificationCheckAuditSnapshotResponse(
    Guid AuditEventId,
    string SnapshotKind,
    DateTimeOffset CapturedAt);

public sealed record QualificationLocalStateResponse(
    Guid? QualificationIssueId,
    string Status,
    string Message,
    string? QualificationName = null,
    DateTimeOffset? IssuedAt = null,
    DateTimeOffset? ExpiresAt = null,
    DateTimeOffset? LastVerifiedAt = null);

public sealed record ComplianceCoreCheckSummaryResponse(
    string RulePackKey,
    string Outcome,
    string ReasonCode,
    string Message,
    string EvaluationResult,
    IReadOnlyList<string> UnresolvedFactKeys,
    Guid? AppliedWaiverId = null,
    string? AppliedWaiverKey = null);

public sealed record BatchQualificationCheckSubject(
    Guid StaffarrPersonId,
    IReadOnlyDictionary<string, string>? Context);

public sealed record CreateBatchQualificationCheckRequest(
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyList<BatchQualificationCheckSubject> Subjects,
    DateTimeOffset? EffectiveAt = null,
    Guid? TrainingDefinitionId = null,
    Guid? TrainingProgramId = null);

public sealed record CreateIntegrationBatchQualificationCheckRequest(
    Guid TenantId,
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyList<BatchQualificationCheckSubject> Subjects,
    DateTimeOffset? EffectiveAt = null,
    Guid? TrainingDefinitionId = null,
    Guid? TrainingProgramId = null);

public sealed record BatchQualificationCheckSummary(
    int Total,
    int AllowCount,
    int WarnCount,
    int BlockCount,
    int WaivedCount = 0);

public sealed record BatchQualificationCheckResponse(
    Guid BatchId,
    string QualificationKey,
    IReadOnlyList<QualificationCheckResponse> Results,
    BatchQualificationCheckSummary Summary,
    QualificationCheckAuditSnapshotResponse? AuditSnapshot = null);

public sealed record QualificationCheckHistoryItemResponse(
    Guid CheckId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string Outcome,
    string ReasonCode,
    string Message,
    string? RulePackKey,
    Guid? TrainingDefinitionId,
    Guid? BatchId,
    DateTimeOffset CheckedAt);

public static class QualificationCheckOutcomes
{
    public const string Allow = "allow";

    public const string Warn = "warn";

    public const string Block = "block";

    public const string Waived = "waived";
}
