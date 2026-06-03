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
    AssetReadinessSignalCountsResponse Signals,
    AssetReadinessDispatchabilitySummaryResponse? Dispatchability = null,
    AssetReadinessConfidenceResponse? Confidence = null,
    AssetReadinessAuditSnapshotResponse? AuditSnapshot = null,
    IReadOnlyList<AssetReadinessComplianceCoreReferenceResponse>? ComplianceCoreReferences = null);

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

public sealed record AssetReadinessDispatchabilitySummaryResponse(
    bool IsDispatchable,
    string Outcome,
    string ReasonCode,
    string Message,
    int BlockerCount,
    string? PrimaryBlockerType,
    string? PrimaryBlockerMessage);

public sealed record AssetReadinessConfidenceResponse(
    string DataSource,
    string FreshnessStatus,
    int? StalenessThresholdHours,
    DateTimeOffset CalculatedAt);

public sealed record AssetReadinessAuditSnapshotResponse(
    Guid AuditEventId,
    string SnapshotKind,
    DateTimeOffset CapturedAt);

public sealed record AssetReadinessComplianceCoreReferenceResponse(
    string ReferenceType,
    string ReferenceId,
    string Outcome,
    string? DeepLinkPath);

public sealed record AssetReadinessHistoryResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    int TotalCount,
    int Limit,
    IReadOnlyList<AssetReadinessHistoryItemResponse> Items);

public sealed record AssetReadinessHistoryItemResponse(
    Guid EntryId,
    string StatusFieldKey,
    string StatusValueKey,
    string? Notes,
    string? ChangedByPersonId,
    DateTimeOffset ChangedAt,
    DateTimeOffset CreatedAt);
