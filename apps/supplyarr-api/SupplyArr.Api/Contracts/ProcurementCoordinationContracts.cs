namespace SupplyArr.Api.Contracts;

public sealed record ProcurementCoordinationSummaryResponse(
    Guid CoordinationRecordId,
    string SubjectType,
    Guid SubjectId,
    string DocumentKey,
    string Title,
    string CoordinationStage,
    string NextActionRequired,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    Guid? VendorPartyId,
    string VendorDisplayName,
    string DocumentStatus,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    int? ReceiptProgressPercent,
    bool IsTerminal,
    DateTimeOffset SourceUpdatedAt,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record ProcurementCoordinationEventResponse(
    string EventKind,
    string Title,
    string? Detail,
    DateTimeOffset OccurredAt,
    int SequenceNumber,
    string SourceEntityType,
    string SourceEntityId);

public sealed record ProcurementCoordinationDetailResponse(
    ProcurementCoordinationSummaryResponse Summary,
    IReadOnlyList<ProcurementCoordinationEventResponse> Events);

public sealed record ProcurementCoordinationStageSummaryResponse(
    string CoordinationStage,
    int Count);

public sealed record ProcurementCoordinationDashboardResponse(
    int ActiveCount,
    int TerminalCount,
    IReadOnlyList<ProcurementCoordinationStageSummaryResponse> StageCounts,
    IReadOnlyList<ProcurementCoordinationSummaryResponse> Items);

public sealed record ProcurementCoordinationSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertProcurementCoordinationSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingProcurementCoordinationItem(
    string SubjectType,
    Guid SubjectId,
    string DocumentKey,
    string Title,
    string DocumentStatus,
    DateTimeOffset SourceUpdatedAt,
    DateTimeOffset? LastComputedAt);

public sealed record PendingProcurementCoordinationResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingProcurementCoordinationItem> Items);

public sealed record ProcurementCoordinationRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record ProcurementCoordinationRunsResponse(
    IReadOnlyList<ProcurementCoordinationRunItem> Items);

public sealed record ProcessProcurementCoordinationRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record ProcurementCoordinationRefreshSkip(
    string SubjectType,
    Guid SubjectId,
    string Reason);

public sealed record ProcessProcurementCoordinationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    IReadOnlyList<ProcurementCoordinationSummaryResponse> Refreshed,
    IReadOnlyList<ProcurementCoordinationRefreshSkip> Skipped);
