namespace ComplianceCore.Api.Contracts;

public sealed record EvaluateControlEffectivenessRequest(
    string? ScopeKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record ControlEffectivenessRecordResponse(
    Guid RecordId,
    Guid RunId,
    string ScopeKey,
    Guid RulePackId,
    string PackKey,
    int EffectivenessScore,
    string EffectivenessLevel,
    string ControlStatus,
    string RuleOutcome,
    string EvaluationResult,
    int TotalRuleCount,
    int PassedRuleCount,
    int FailedRuleCount,
    int UnresolvedFactCount,
    int ResolvedFactCount,
    string Summary,
    DateTimeOffset EvaluatedAt);

public sealed record EvaluateControlEffectivenessResponse(
    Guid RunId,
    string ScopeKey,
    int PacksEvaluatedCount,
    int LowestEffectivenessScore,
    string LowestEffectivenessLevel,
    int AverageEffectivenessScore,
    DateTimeOffset EvaluatedAt,
    IReadOnlyList<ControlEffectivenessRecordResponse> Records);

public sealed record ControlEffectivenessSummaryResponse(
    int TotalControls,
    int ScopesTracked,
    int EffectiveCount,
    int PartiallyEffectiveCount,
    int IneffectiveCount,
    int UnknownCount,
    int LowestEffectivenessScore,
    string LowestEffectivenessLevel,
    int AverageEffectivenessScore,
    DateTimeOffset? LastEvaluatedAt,
    DateTimeOffset GeneratedAt);
