namespace ComplianceCore.Api.Contracts;

public sealed record InternalEvaluateRulePackRequest(
    Guid TenantId,
    string RulePackKey,
    IReadOnlyDictionary<string, string>? Context,
    bool EmitFindings = false);

public sealed record InternalEvaluateRulePackResponse(
    Guid TenantId,
    Guid RulePackId,
    string RulePackKey,
    string Outcome,
    string ReasonCode,
    string Message,
    string EvaluationResult,
    IReadOnlyList<string> UnresolvedFactKeys,
    IReadOnlyDictionary<string, bool> ResolvedFacts,
    IReadOnlyList<RuleEvaluationItemResponse> RuleResults,
    Guid? EvaluationRunId = null,
    IReadOnlyList<ComplianceFindingResponse> FindingsEmitted = null!,
    Guid? AppliedWaiverId = null,
    string? AppliedWaiverKey = null);

public sealed record InternalEvaluateRulePackBatchItem(
    string RulePackKey,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record InternalEvaluateRulePackBatchRequest(
    Guid TenantId,
    IReadOnlyList<InternalEvaluateRulePackBatchItem> Items,
    IReadOnlyDictionary<string, string>? Context = null,
    bool EmitFindings = false);

public sealed record InternalEvaluateRulePackBatchSummary(
    int Total,
    int AllowCount,
    int WarnCount,
    int BlockCount,
    int WaivedCount);

public sealed record InternalEvaluateRulePackBatchResponse(
    Guid BatchId,
    IReadOnlyList<InternalEvaluateRulePackResponse> Results,
    InternalEvaluateRulePackBatchSummary Summary);

public static class ComplianceEvaluationOutcomes
{
    public const string Allow = "allow";

    public const string Warn = "warn";

    public const string Block = "block";

    public const string Waived = "waived";
}
