namespace StaffArr.Api.Contracts;

public sealed record ReadinessRollupSummaryResponse(
    Guid OrgUnitId,
    string ScopeType,
    string OrgUnitName,
    int TotalMembers,
    int ReadyCount,
    int NotReadyCount,
    int OverrideCount,
    decimal ReadyPercent,
    string ConfidenceLevel,
    int ConfidenceScore,
    DateTimeOffset ComputedAt);

public sealed record ReadinessRollupMemberResponse(
    Guid PersonId,
    string DisplayName,
    string ReadinessStatus,
    string ReadinessBasis,
    bool HasActiveOverride,
    int BlockerCount,
    string? PrimaryBlockerMessage);

public sealed record ReadinessRollupMembersResponse(
    ReadinessRollupSummaryResponse Rollup,
    IReadOnlyList<ReadinessRollupMemberResponse> Members);

public sealed record PendingReadinessRollupItem(
    Guid OrgUnitId,
    string ScopeType,
    string OrgUnitName,
    DateTimeOffset? LastComputedAt);

public sealed record PendingReadinessRollupsResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingReadinessRollupItem> Items);

public sealed record ProcessReadinessRollupsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record ReadinessRollupRefreshSkip(
    Guid OrgUnitId,
    string ScopeType,
    string Reason);

public sealed record ProcessReadinessRollupsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    IReadOnlyList<ReadinessRollupSummaryResponse> RefreshedRollups,
    IReadOnlyList<ReadinessRollupRefreshSkip> Skipped);
