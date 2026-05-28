namespace MaintainArr.Api.Contracts;

public sealed record PmDueScanSettingsResponse(
    bool IsEnabled,
    int ScanIntervalMinutes,
    int BatchSize,
    int OverdueGraceDays,
    DateTimeOffset? LastRunAt,
    int PendingPmCount,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertPmDueScanSettingsRequest(
    bool IsEnabled,
    int ScanIntervalMinutes,
    int BatchSize,
    int OverdueGraceDays);

public sealed record PmDueScanRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int MarkedDueCount,
    int MarkedOverdueCount,
    int SkippedCount,
    int WorkOrdersCreatedCount,
    int WorkOrdersLinkedCount,
    DateTimeOffset CreatedAt);

public sealed record PmDueScanRunsResponse(IReadOnlyList<PmDueScanRunItem> Items);

public sealed record TriggerPmDueScanResponse(ProcessPmDueScanResponse Result);
