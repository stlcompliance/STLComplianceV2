namespace SupplyArr.Api.Contracts;

public sealed record DemandProcessingSettingsResponse(
    bool IsEnabled,
    bool AutoCreatePrDraftWhenShort,
    int MinHoursBeforeProcessing,
    int StalenessHours,
    bool NotifyOnPrDraftCreated,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertDemandProcessingSettingsRequest(
    bool IsEnabled,
    bool AutoCreatePrDraftWhenShort,
    int MinHoursBeforeProcessing,
    int StalenessHours,
    bool NotifyOnPrDraftCreated);

public sealed record PendingDemandProcessingItem(
    Guid DemandRefId,
    string MaintainarrWorkOrderNumber,
    string Title,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? LastProcessedAt,
    string? LastProcessingOutcome);

public sealed record PendingDemandProcessingResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingDemandProcessingItem> Items);

public sealed record DemandProcessingRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int ProcessedCount,
    int PrDraftsCreatedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record DemandProcessingRunsResponse(
    IReadOnlyList<DemandProcessingRunItem> Items);

public sealed record ProcessDemandProcessingRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record DemandProcessingLineSummary(
    Guid LineId,
    int LineNumber,
    Guid? PartId,
    string PartNumber,
    decimal QuantityRequested,
    decimal QuantityAvailable,
    bool IsShort);

public sealed record DemandProcessingResult(
    Guid DemandRefId,
    string MaintainarrWorkOrderNumber,
    string ProcessingOutcome,
    string RecommendedAction,
    int LinesShortCount,
    Guid? PurchaseRequestId,
    Guid? NotificationDispatchId);

public sealed record DemandProcessingSkip(
    Guid DemandRefId,
    string Reason);

public sealed record ProcessDemandProcessingResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int ProcessedCount,
    int PrDraftsCreatedCount,
    int SkippedCount,
    IReadOnlyList<DemandProcessingResult> Processed,
    IReadOnlyList<DemandProcessingSkip> Skipped);

public sealed record DemandProcessingSummaryResponse(
    Guid ProcessingStateId,
    Guid DemandRefId,
    string MaintainarrWorkOrderNumber,
    string Title,
    string DemandRefStatus,
    string ProcessingOutcome,
    string RecommendedAction,
    int LinesTotalCount,
    int LinesCatalogCount,
    int LinesShortCount,
    Guid? PurchaseRequestId,
    string? LastProcessingMessage,
    DateTimeOffset DemandReceivedAt,
    DateTimeOffset LastProcessedAt);

public sealed record DemandProcessingDashboardResponse(
    int PendingCount,
    int StockShortCount,
    int StockAvailableCount,
    int PrDraftedCount,
    IReadOnlyList<DemandProcessingSummaryResponse> Items);

public sealed record DemandProcessingDetailResponse(
    DemandProcessingSummaryResponse Summary,
    IReadOnlyList<DemandProcessingLineSummary> Lines);
