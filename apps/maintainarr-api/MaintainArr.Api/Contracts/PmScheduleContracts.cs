namespace MaintainArr.Api.Contracts;

public sealed record PmScheduleResponse(
    Guid PmScheduleId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string ScheduleKey,
    string Name,
    string Description,
    string ScheduleMode,
    Guid? AssetMeterId,
    string? MeterKey,
    string? MeterUnit,
    decimal? IntervalUsage,
    decimal? NextDueAtUsage,
    decimal? LastCompletedUsage,
    int IntervalDays,
    DateTimeOffset NextDueAt,
    DateTimeOffset? LastCompletedAt,
    string DueStatus,
    string Status,
    DateTimeOffset? LastDueScanAt,
    Guid? LinkedWorkOrderId,
    string? LinkedWorkOrderNumber,
    string? LinkedWorkOrderStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePmScheduleRequest(
    Guid AssetId,
    string ScheduleKey,
    string Name,
    string Description,
    int IntervalDays,
    DateTimeOffset NextDueAt,
    string? ScheduleMode = null,
    Guid? AssetMeterId = null,
    decimal? IntervalUsage = null,
    decimal? NextDueAtUsage = null);

public sealed record UpdatePmScheduleRequest(
    string Name,
    string Description,
    int IntervalDays,
    DateTimeOffset NextDueAt,
    string? ScheduleMode = null,
    Guid? AssetMeterId = null,
    decimal? IntervalUsage = null,
    decimal? NextDueAtUsage = null);

public sealed record UpdatePmScheduleStatusRequest(string Status);

public sealed record PendingPmDueItem(
    Guid PmScheduleId,
    Guid TenantId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string ScheduleKey,
    string DueStatus,
    DateTimeOffset NextDueAt);

public sealed record PendingPmDueResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingPmDueItem> Items);

public sealed record ProcessPmDueScanRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? OverdueGraceDays);

public sealed record PmDueScanSkip(
    Guid PmScheduleId,
    string Reason);

public sealed record PmWorkOrderGenerationSkip(
    Guid PmScheduleId,
    string Reason);

public sealed record ProcessPmDueScanResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int MarkedDueCount,
    int MarkedOverdueCount,
    int SkippedCount,
    int WorkOrdersCreatedCount,
    int WorkOrdersLinkedCount,
    int WorkOrderGenerationSkippedCount,
    IReadOnlyList<Guid> UpdatedPmScheduleIds,
    IReadOnlyList<Guid> CreatedWorkOrderIds,
    IReadOnlyList<PmDueScanSkip> Skipped,
    IReadOnlyList<PmWorkOrderGenerationSkip> WorkOrderGenerationSkipped);
