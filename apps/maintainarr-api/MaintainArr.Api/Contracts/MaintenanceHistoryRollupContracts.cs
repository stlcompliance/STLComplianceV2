namespace MaintainArr.Api.Contracts;

public sealed record MaintenanceHistorySummaryResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    int EventCount,
    int InspectionCount,
    int DefectCount,
    int WorkOrderCount,
    int PmCount,
    DateTimeOffset? LastEventAt,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record MaintenanceHistoryRollupSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertMaintenanceHistoryRollupSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingMaintenanceHistoryRollupItem(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    DateTimeOffset? LastComputedAt);

public sealed record PendingMaintenanceHistoryRollupsResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingMaintenanceHistoryRollupItem> Items);

public sealed record MaintenanceHistoryRollupRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record MaintenanceHistoryRollupRunsResponse(
    IReadOnlyList<MaintenanceHistoryRollupRunItem> Items);

public sealed record ProcessMaintenanceHistoryRollupsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record MaintenanceHistoryRollupRefreshSkip(
    Guid AssetId,
    string Reason);

public sealed record ProcessMaintenanceHistoryRollupsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    IReadOnlyList<MaintenanceHistorySummaryResponse> Refreshed,
    IReadOnlyList<MaintenanceHistoryRollupRefreshSkip> Skipped);
