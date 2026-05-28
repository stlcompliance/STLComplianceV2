namespace ComplianceCore.Api.Contracts;

public sealed record M12AnalyticsWorkerSettingsResponse(
    bool IsEnabled,
    string DefaultScopeKey,
    int IntervalHours,
    bool RiskScoringEnabled,
    bool MissingEvidenceEnabled,
    bool ControlEffectivenessEnabled,
    bool ReadinessForecastEnabled,
    bool AuditDeliveryEnabled,
    DateTimeOffset? LastBatchRunAt,
    DateTimeOffset? LastRiskScoringRunAt,
    DateTimeOffset? LastMissingEvidenceRunAt,
    DateTimeOffset? LastControlEffectivenessRunAt,
    DateTimeOffset? LastReadinessForecastRunAt,
    DateTimeOffset? LastAuditDeliveryRunAt,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertM12AnalyticsWorkerSettingsRequest(
    bool IsEnabled,
    string? DefaultScopeKey,
    int? IntervalHours,
    bool? RiskScoringEnabled,
    bool? MissingEvidenceEnabled,
    bool? ControlEffectivenessEnabled,
    bool? ReadinessForecastEnabled,
    bool? AuditDeliveryEnabled);

public sealed record PendingM12AnalyticsBatchTenantItem(
    Guid TenantId,
    string DefaultScopeKey,
    int IntervalHours,
    bool RiskScoringDue,
    bool MissingEvidenceDue,
    bool ControlEffectivenessDue,
    bool ReadinessForecastDue,
    bool AuditDeliveryDue);

public sealed record PendingM12AnalyticsBatchesResponse(
    DateTimeOffset AsOf,
    int IntervalHours,
    int BatchSize,
    IReadOnlyList<PendingM12AnalyticsBatchTenantItem> Items);

public sealed record ProcessM12AnalyticsBatchesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? IntervalHours,
    int? BatchSize);

public sealed record M12AnalyticsBatchRunResult(
    Guid RunId,
    Guid TenantId,
    string Status,
    bool RiskScoringRan,
    bool MissingEvidenceRan,
    bool ControlEffectivenessRan,
    bool ReadinessForecastRan,
    bool AuditDeliveryQueued,
    Guid? AuditPackageJobId,
    string? ErrorMessage);

public sealed record ProcessM12AnalyticsBatchesResponse(
    DateTimeOffset AsOf,
    int IntervalHours,
    int BatchSize,
    int TenantsDueCount,
    int ProcessedCount,
    int SkippedCount,
    IReadOnlyList<M12AnalyticsBatchRunResult> Results);
