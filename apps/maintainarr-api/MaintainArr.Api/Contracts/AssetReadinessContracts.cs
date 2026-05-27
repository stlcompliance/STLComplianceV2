namespace MaintainArr.Api.Contracts;

public sealed record AssetReadinessResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string LifecycleStatus,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<AssetReadinessBlockerResponse> Blockers,
    AssetReadinessSignalCountsResponse Signals);

public sealed record AssetReadinessSummaryResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string LifecycleStatus,
    string ReadinessStatus,
    int BlockerCount,
    string? PrimaryBlockerMessage);

public sealed record AssetReadinessBlockerResponse(
    string BlockerType,
    string Message,
    string SourceEntityType,
    string SourceEntityId,
    string? RelatedEntityId);

public sealed record AssetReadinessSignalCountsResponse(
    int OpenCriticalDefectCount,
    int OpenHighDefectCount,
    int ActiveWorkOrderCount,
    int PmDueCount,
    int PmOverdueCount,
    int FailedInspectionCount);
