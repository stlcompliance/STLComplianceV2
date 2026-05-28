namespace ComplianceCore.Api.Contracts;

public sealed record ScheduledRuleEvaluationRunSummary(
    Guid RunId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Status,
    int PacksDueCount,
    int EvaluatedCount,
    int SkippedCount,
    int AllowCount,
    int WarnCount,
    int BlockCount);

public sealed record M12AnalyticsBatchRunSummary(
    Guid RunId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Status,
    string ScopeKey,
    bool RiskScoringRan,
    bool MissingEvidenceRan,
    bool ControlEffectivenessRan,
    bool ReadinessForecastRan,
    bool AuditDeliveryQueued,
    Guid? AuditPackageJobId,
    string? ErrorMessage);

public sealed record AuditPackageJobSummary(
    Guid JobId,
    string Status,
    string Format,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    Guid? PackageId,
    string? ErrorMessage);

public sealed record AuditDeliveryScheduledEvaluationStatus(
    int PendingPacksCount,
    ScheduledRuleEvaluationRunSummary? LastRun);

public sealed record AuditDeliveryM12BatchStatus(
    bool WorkerEnabled,
    bool BatchDue,
    PendingM12AnalyticsBatchTenantItem? PendingSteps,
    M12AnalyticsBatchRunSummary? LastRun);

public sealed record AuditDeliveryAuditPackageStatus(
    int PendingJobsCount,
    IReadOnlyList<AuditPackageJobSummary> RecentJobs);

public sealed record AuditDeliveryOrchestrationStatusResponse(
    M12AnalyticsWorkerSettingsResponse WorkerSettings,
    AuditDeliveryScheduledEvaluationStatus ScheduledEvaluation,
    AuditDeliveryM12BatchStatus M12Batch,
    AuditDeliveryAuditPackageStatus AuditPackages);

public sealed record TriggerScheduledRuleEvaluationResponse(
    Guid ScheduledRunId,
    int EvaluatedCount,
    int SkippedCount,
    int AllowCount,
    int WarnCount,
    int BlockCount);

public sealed record TriggerM12AnalyticsBatchResponse(
    Guid? BatchRunId,
    string Status,
    bool AuditDeliveryQueued,
    Guid? AuditPackageJobId,
    string? ErrorMessage);
