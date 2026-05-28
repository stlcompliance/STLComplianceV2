namespace ComplianceCore.Api.Contracts;

public sealed record EvaluateRiskScoresRequest(
    string? ScopeKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record RiskScoreResponse(
    Guid RiskScoreId,
    Guid RunId,
    string ScopeKey,
    Guid RulePackId,
    string PackKey,
    int RiskScore,
    string RiskLevel,
    string RuleOutcome,
    string EvaluationResult,
    int UnresolvedFactCount,
    int FailedRuleCount,
    int ResolvedFactCount,
    int MirrorFactCount,
    string Summary,
    DateTimeOffset EvaluatedAt);

public sealed record EvaluateRiskScoresResponse(
    Guid RunId,
    string ScopeKey,
    int PacksEvaluatedCount,
    int HighestRiskScore,
    string HighestRiskLevel,
    int MirrorFactCount,
    DateTimeOffset EvaluatedAt,
    IReadOnlyList<RiskScoreResponse> Scores);

public sealed record RiskScoreSummaryResponse(
    int TotalScores,
    int ScopesTracked,
    int LowCount,
    int MediumCount,
    int HighCount,
    int CriticalCount,
    int HighestRiskScore,
    string HighestRiskLevel,
    DateTimeOffset? LastEvaluatedAt,
    DateTimeOffset GeneratedAt);
