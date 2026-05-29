namespace ComplianceCore.Api.Contracts;

public sealed record FactSourceSyncWorkerSettingsResponse(
    bool IsEnabled,
    string DefaultScopeKey,
    int IntervalMinutes,
    DateTimeOffset? LastBatchRunAt,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertFactSourceSyncWorkerSettingsRequest(
    bool IsEnabled,
    string DefaultScopeKey,
    int IntervalMinutes);

public sealed record PendingFactSourceSyncItem(
    Guid FactSourceId,
    Guid TenantId,
    string SourceKey,
    string FactKey,
    string? ProductKey,
    string ScopeKey,
    string HealthStatus,
    DateTimeOffset? LastSuccessAt);

public sealed record PendingFactSourceSyncsResponse(
    DateTimeOffset AsOf,
    int IntervalMinutes,
    int BatchSize,
    IReadOnlyList<PendingFactSourceSyncItem> Items);

public sealed record ProcessFactSourceSyncsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? IntervalMinutes,
    int? BatchSize);

public sealed record FactSourceSyncRunResult(
    Guid FactSourceId,
    string SourceKey,
    string FactKey,
    string Status,
    string? ErrorMessage,
    Guid? MirrorId);

public sealed record ProcessFactSourceSyncsResponse(
    DateTimeOffset AsOf,
    int IntervalMinutes,
    int BatchSize,
    int DueCount,
    int SucceededCount,
    int FailedCount,
    int SkippedCount,
    IReadOnlyList<FactSourceSyncRunResult> Results);

public sealed record FactSourceSyncHealthItem(
    Guid FactSourceId,
    string SourceKey,
    string FactKey,
    string SourceType,
    string? ProductKey,
    string ScopeKey,
    string HealthStatus,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? LastErrorMessage,
    int ConsecutiveFailureCount);

public sealed record FactSourceSyncHealthResponse(
    Guid TenantId,
    bool WorkerEnabled,
    int IntervalMinutes,
    DateTimeOffset? LastBatchRunAt,
    int ProductApiSourceCount,
    int HealthyCount,
    int StaleCount,
    int FailedCount,
    int PendingCount,
    IReadOnlyList<FactSourceSyncHealthItem> Sources);
