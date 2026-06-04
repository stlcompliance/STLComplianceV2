namespace ComplianceCore.Api.Contracts;

public sealed record EvaluationHistoryExplorerResponse(
    Guid TenantId,
    Guid? RulePackId,
    string? PackKey,
    int TotalRuns,
    int PassedCount,
    int FailedCount,
    int Limit,
    int Offset,
    bool HasMore,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<EvaluationHistoryExplorerItemResponse> Runs);

public sealed record EvaluationHistoryExplorerItemResponse(
    Guid EvaluationRunId,
    Guid RulePackId,
    string PackKey,
    string PackLabel,
    string OverallResult,
    string Status,
    Guid? ActorUserId,
    DateTimeOffset CreatedAt);
