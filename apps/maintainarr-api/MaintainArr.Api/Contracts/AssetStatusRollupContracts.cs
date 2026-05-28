namespace MaintainArr.Api.Contracts;

public sealed record AssetStatusRollupSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertAssetStatusRollupSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingAssetStatusRollupItem(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    DateTimeOffset? LastComputedAt);

public sealed record PendingAssetStatusRollupsResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingAssetStatusRollupItem> Items);

public sealed record AssetStatusRollupRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    int ScopeRollupsRefreshed,
    DateTimeOffset CreatedAt);

public sealed record AssetStatusRollupRunsResponse(
    IReadOnlyList<AssetStatusRollupRunItem> Items);

public sealed record AssetStatusRollupSummaryResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string LifecycleStatus,
    string ReadinessStatus,
    int BlockerCount,
    string? PrimaryBlockerMessage,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record AssetStatusScopeRollupSummaryResponse(
    string ScopeType,
    Guid ScopeEntityId,
    string? ScopeEntityKey,
    string ScopeLabel,
    int TotalAssets,
    int ReadyCount,
    int NotReadyCount,
    decimal ReadyPercent,
    DateTimeOffset ComputedAt);

public sealed record ProcessAssetStatusRollupsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record AssetStatusRollupRefreshSkip(
    Guid AssetId,
    string Reason);

public sealed record ProcessAssetStatusRollupsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    int ScopeRollupsRefreshed,
    IReadOnlyList<AssetStatusRollupSummaryResponse> Refreshed,
    IReadOnlyList<AssetStatusRollupRefreshSkip> Skipped);
