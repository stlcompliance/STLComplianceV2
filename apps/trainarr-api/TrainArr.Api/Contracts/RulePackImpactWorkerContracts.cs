namespace TrainArr.Api.Contracts;

public sealed record UpsertRulePackImpactSettingsRequest(
    bool IsEnabled,
    int StalenessHours,
    bool AutoUpdateRequirementBaselines);

public sealed record RulePackImpactSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    bool AutoUpdateRequirementBaselines,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessRulePackImpactScansRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record PendingRulePackImpactItem(
    string RulePackKey,
    Guid TenantId,
    DateTimeOffset? LastComputedAt);

public sealed record PendingRulePackImpactScansResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingRulePackImpactItem> Items);

public sealed record RulePackImpactScanSkip(
    string RulePackKey,
    string Reason);

public sealed record ProcessRulePackImpactScansResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int AssessedCount,
    int AttentionRequiredCount,
    int SkippedCount,
    IReadOnlyList<string> AssessedRulePackKeys,
    IReadOnlyList<string> AttentionRequiredRulePackKeys,
    IReadOnlyList<RulePackImpactScanSkip> Skipped);

public sealed record RulePackImpactStateItem(
    string RulePackKey,
    bool RequiresAttention,
    bool HasDrift,
    IReadOnlyList<string> Triggers,
    int? BaselineVersionNumber,
    int? CurrentVersionNumber,
    string? BaselineStatus,
    string? CurrentStatus,
    int ActiveAssignmentCount,
    int ActiveQualificationCount,
    DateTimeOffset ComputedAt);

public sealed record RulePackImpactStatesResponse(
    IReadOnlyList<RulePackImpactStateItem> Items);

public sealed record RulePackImpactRunItem(
    Guid RunId,
    string RulePackKey,
    string Outcome,
    bool RequiresAttention,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record RulePackImpactRunsResponse(
    IReadOnlyList<RulePackImpactRunItem> Items);
