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
    bool ExpectedValue,
    bool NonWaivable = false,
    bool RemediationRequired = false,
    bool ReviewRequired = false);

public sealed record UpdateRulePackContentRequest(RulePackContentBody Content);

public sealed record EvaluateRulePackRequest(
    IReadOnlyDictionary<string, bool> Facts,
    bool EmitFindings = false);

public sealed record EvaluateRulePackRunRequest(
    Guid RulePackId,
    IReadOnlyDictionary<string, bool> Facts,
    bool EmitFindings = false);

public sealed record ReEvaluateRuleEvaluationRequest(
    bool EmitFindings = false);

public sealed record EvaluateRulePackSimulationRequest(
    Guid RulePackId,
    IReadOnlyDictionary<string, bool> Facts);

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
    IReadOnlyList<ComplianceFindingResponse> FindingsEmitted = null!,
    Guid? AppliedWaiverId = null,
    string? AppliedWaiverKey = null);

public sealed record RuleEvaluationItemResponse(
    string RuleKey,
    string Label,
    string Result,
    string Message,
    bool NonWaivable = false,
    bool RemediationRequired = false,
    bool ReviewRequired = false);

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

public sealed record RuleEvaluationAuditExportResponse(
    Guid ExportId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    AuditPackageEvaluationRunItem EvaluationRun,
    IReadOnlyList<AuditPackageWorkflowGateCheckItem> WorkflowGateChecks,
    IReadOnlyList<AuditPackageFindingItem> Findings,
    IReadOnlyList<AuditPackageWaiverItem> Waivers);

public sealed record RuleEvaluationSimulationResponse(
    Guid RulePackId,
    string PackKey,
    string PackLabel,
    int VersionNumber,
    string OverallResult,
    IReadOnlyDictionary<string, bool> FactInputs,
    IReadOnlyList<RuleEvaluationItemResponse> RuleResults,
    DateTimeOffset EvaluatedAt);

public sealed record RuleEvaluationExplanationResponse(
    Guid EvaluationRunId,
    Guid RulePackId,
    string PackKey,
    string OverallResult,
    string Summary,
    IReadOnlyList<string> FailedRuleKeys,
    IReadOnlyList<string> MissingFactKeys,
    IReadOnlyList<RuleEvaluationItemResponse> RuleResults,
    DateTimeOffset GeneratedAt);
