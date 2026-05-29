namespace MaintainArr.Api.Contracts;

public sealed record DowntimeTrackingSettingsResponse(
    bool IsEnabled,
    bool AutoTrackOutOfService,
    bool AutoTrackNotReady,
    int AvailabilityPeriodDays,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertDowntimeTrackingSettingsRequest(
    bool IsEnabled,
    bool AutoTrackOutOfService,
    bool AutoTrackNotReady,
    int AvailabilityPeriodDays);

public sealed record AssetDowntimeEventResponse(
    Guid EventId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string Source,
    string Reason,
    bool IsPlanned,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string? StatusTrigger,
    Guid? WorkOrderId,
    Guid? DefectId,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateManualDowntimeEventRequest(
    Guid AssetId,
    string Reason,
    bool IsPlanned,
    DateTimeOffset StartedAt,
    string? Notes,
    Guid? WorkOrderId,
    Guid? DefectId);

public sealed record CloseDowntimeEventRequest(
    DateTimeOffset? EndedAt,
    string? Notes);

public sealed record AssetAvailabilityResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal TotalHours,
    decimal DowntimeHours,
    decimal AvailabilityPercent,
    decimal PlannedDowntimeHours,
    decimal UnplannedDowntimeHours,
    bool HasActiveDowntime,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record FleetAvailabilityResponse(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    int AssetCount,
    decimal TotalHours,
    decimal DowntimeHours,
    decimal AvailabilityPercent,
    decimal PlannedDowntimeHours,
    decimal UnplannedDowntimeHours,
    int ActiveDowntimeEventCount,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record PendingAssetDowntimeSyncItem(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string LifecycleStatus,
    string ReadinessStatus,
    bool HasOpenAutomaticEvent);

public sealed record PendingAssetDowntimeSyncResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingAssetDowntimeSyncItem> Items);

public sealed record AssetDowntimeSyncRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int AssetsScanned,
    int EventsOpened,
    int EventsClosed,
    int SnapshotsRefreshed,
    DateTimeOffset CreatedAt);

public sealed record AssetDowntimeSyncRunsResponse(
    IReadOnlyList<AssetDowntimeSyncRunItem> Items);

public sealed record ProcessAssetDowntimeSyncRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessAssetDowntimeSyncResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int AssetsScanned,
    int EventsOpened,
    int EventsClosed,
    int SnapshotsRefreshed,
    IReadOnlyList<AssetAvailabilityResponse> RefreshedAssets);
