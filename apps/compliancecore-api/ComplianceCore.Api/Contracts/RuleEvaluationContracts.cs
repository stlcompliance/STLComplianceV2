namespace ComplianceCore.Api.Contracts;

public sealed record RulePackContentResponse(
    Guid RulePackId,
    string PackKey,
    int VersionNumber,
    string Status,
    bool HasContent,
    RulePackContentBody? Content,
    DateTimeOffset UpdatedAt);

public sealed record RulePackContentBody(
    int SchemaVersion,
    string Logic,
    IReadOnlyList<RuleDefinitionDto> Rules);

public sealed record RuleDefinitionDto(
    string RuleKey,
    string Label,
    string Type,
    string FactKey,
    bool ExpectedValue);

public sealed record UpdateRulePackContentRequest(RulePackContentBody Content);

public sealed record EvaluateRulePackRequest(
    IReadOnlyDictionary<string, bool> Facts,
    bool EmitFindings = false);

public sealed record RuleEvaluationRunResponse(
    Guid EvaluationRunId,
    Guid RulePackId,
    string PackKey,
    string PackLabel,
    int VersionNumber,
    string Status,
    string OverallResult,
    IReadOnlyDictionary<string, bool> FactInputs,
    IReadOnlyList<RuleEvaluationItemResponse> RuleResults,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ComplianceFindingResponse> FindingsEmitted = null!);

public sealed record RuleEvaluationItemResponse(
    string RuleKey,
    string Label,
    string Result,
    string Message);

public sealed record EvaluateRulePackBatchItem(
    string RulePackKey,
    IReadOnlyDictionary<string, bool>? Facts = null);

public sealed record EvaluateRulePackBatchRequest(
    IReadOnlyList<EvaluateRulePackBatchItem> Items,
    IReadOnlyDictionary<string, bool>? Facts = null,
    bool EmitFindings = false);

public sealed record EvaluateRulePackBatchSummary(
    int Total,
    int AllowCount,
    int WarnCount,
    int BlockCount);

public sealed record EvaluateRulePackBatchResultItem(
    string RulePackKey,
    Guid RulePackId,
    string PackLabel,
    string Outcome,
    string ReasonCode,
    string Message,
    string OverallResult,
    Guid? EvaluationRunId,
    IReadOnlyList<RuleEvaluationItemResponse> RuleResults,
    IReadOnlyList<ComplianceFindingResponse> FindingsEmitted);

public sealed record EvaluateRulePackBatchResponse(
    Guid BatchId,
    IReadOnlyList<EvaluateRulePackBatchResultItem> Results,
    EvaluateRulePackBatchSummary Summary);
