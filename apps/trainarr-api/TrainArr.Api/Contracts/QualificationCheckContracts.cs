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
    ComplianceCoreCheckSummaryResponse? ComplianceCore);

public sealed record QualificationLocalStateResponse(
    Guid? QualificationIssueId,
    string Status,
    string Message);

public sealed record ComplianceCoreCheckSummaryResponse(
    string RulePackKey,
    string Outcome,
    string ReasonCode,
    string Message,
    string EvaluationResult,
    IReadOnlyList<string> UnresolvedFactKeys);

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

public sealed record BatchQualificationCheckSummary(
    int Total,
    int AllowCount,
    int WarnCount,
    int BlockCount);

public sealed record BatchQualificationCheckResponse(
    Guid BatchId,
    string QualificationKey,
    IReadOnlyList<QualificationCheckResponse> Results,
    BatchQualificationCheckSummary Summary);

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
}
