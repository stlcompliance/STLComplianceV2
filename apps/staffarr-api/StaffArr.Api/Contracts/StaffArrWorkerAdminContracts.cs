namespace StaffArr.Api.Contracts;

public sealed record StaffArrWorkerSettingsResponse(
    string WorkerKey,
    bool IsEnabled,
    int ScanIntervalMinutes,
    int BatchSize,
    int? StalenessHours,
    DateTimeOffset? LastRunAt,
    int PendingCount);

public sealed record UpsertStaffArrWorkerSettingsRequest(
    bool IsEnabled,
    int ScanIntervalMinutes,
    int BatchSize,
    int? StalenessHours);

public sealed record StaffArrWorkerPendingPreviewResponse(
    string WorkerKey,
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int ItemCount,
    IReadOnlyList<string> PreviewLines);

public sealed record StaffArrWorkerRunItem(
    Guid RunId,
    string Status,
    int CandidatesFound,
    int ProcessedCount,
    int SkippedCount,
    string? Summary,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt);

public sealed record StaffArrWorkerRunsResponse(IReadOnlyList<StaffArrWorkerRunItem> Items);

public sealed record PersonExportDeliveryRunItem(
    Guid RunId,
    string Status,
    Guid ExportId,
    int PersonCount,
    int IntervalHours,
    string? SkipReason,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt);

public sealed record PersonExportDeliveryRunsResponse(IReadOnlyList<PersonExportDeliveryRunItem> Items);
