namespace ComplianceCore.Api.Contracts;

public sealed record FindingsReportSummaryResponse(
    int TotalFindings,
    int OpenCount,
    int AcknowledgedCount,
    int ResolvedCount,
    int OpenBlockSeverityCount,
    int OpenWarnSeverityCount,
    IReadOnlyList<FindingsReportSummaryItem> RecentFindings);

public sealed record FindingsReportSummaryItem(
    Guid FindingId,
    string FindingKey,
    string Severity,
    string Status,
    string Title,
    string PackKey,
    DateTimeOffset CreatedAt);

public sealed record OperatorReportSummaryResponse(
    int EvaluationTotalCount,
    int EvaluationPassCount,
    int EvaluationFailCount,
    int EvaluationsLast24Hours,
    int WorkflowGateDefinitionCount,
    int WorkflowGateBlockCount,
    int WorkflowGateWarnCount,
    int RulePackPublishedCount,
    int RulePackDraftCount,
    int AttentionItemCount,
    IReadOnlyList<OperatorReportSummaryItem> RecentEvaluations);

public sealed record OperatorReportSummaryItem(
    Guid EvaluationRunId,
    string RulePackLabel,
    string PackKey,
    string OverallResult,
    DateTimeOffset CreatedAt);

public sealed record OperatorReportAlertResponse(
    string AlertType,
    string Severity,
    Guid? RulePackId,
    string? PackKey,
    Guid? WaiverId,
    string Message,
    DateTimeOffset DetectedAt);

public sealed record MissingEvidenceReportSummaryResponse(
    int TotalWarnings,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int MissingMirrorCount,
    int UnresolvedFactCount,
    int NoFactDefinitionCount,
    IReadOnlyList<MissingEvidenceReportSummaryItem> RecentWarnings);

public sealed record MissingEvidenceReportSummaryItem(
    Guid WarningId,
    Guid RunId,
    Guid RulePackId,
    string PackKey,
    string FactKey,
    string WarningType,
    string Severity,
    string ReasonCode,
    bool HasMirrorAtScope,
    bool IsRequiredInRule,
    bool IsRequiredInCatalog,
    string Summary,
    DateTimeOffset EvaluatedAt);

public sealed record RemediationQueueReportSummaryResponse(
    int TotalWarnings,
    int QueuedCount,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    DateTimeOffset? LastEvaluatedAt,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<RemediationQueueItemResponse> QueueItems);

public sealed record RemediationQueueItemResponse(
    Guid WarningId,
    Guid RunId,
    Guid RulePackId,
    string PackKey,
    string FactKey,
    string WarningType,
    string Severity,
    string ReasonCode,
    string QueueState,
    string RecommendedAction,
    bool HasMirrorAtScope,
    bool IsRequiredInRule,
    bool IsRequiredInCatalog,
    string Summary,
    DateTimeOffset EvaluatedAt);

public sealed record WaiverReportSummaryResponse(
    int TotalWaivers,
    int PendingCount,
    int ApprovedCount,
    int RejectedCount,
    int RevokedCount,
    int ExpiredCount,
    int ExpiringSoonCount,
    IReadOnlyList<WaiverReportSummaryItem> RecentWaivers);

public sealed record WaiverReportSummaryItem(
    Guid WaiverId,
    string WaiverKey,
    string PackKey,
    string SubjectScopeKey,
    string Status,
    string ReasonCode,
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset UpdatedAt);

public sealed record ExceptionExemptionReportSummaryResponse(
    int TotalExceptionExemptions,
    int ActiveCount,
    int InactiveCount,
    int WaiverTypeCount,
    int VarianceTypeCount,
    int SpecialPermitTypeCount,
    int ExpiringSoonCount,
    IReadOnlyList<ExceptionExemptionReportSummaryItem> RecentExceptionExemptions);

public sealed record ExceptionExemptionReportSummaryItem(
    Guid ExceptionExemptionId,
    string Key,
    string Label,
    string Type,
    string EffectType,
    string PackKey,
    string? CitationKey,
    string ActiveState,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset UpdatedAt);

public sealed record ProductIntegrationHealthReportSummaryResponse(
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

public sealed record AuditReadinessReportSummaryResponse(
    int TotalForecasts,
    int ScopesTracked,
    int ReadyCount,
    int CautionCount,
    int NotReadyCount,
    int UnknownCount,
    int ReadinessScore,
    string ReadinessLevel,
    int LowestReadinessScore,
    int AverageReadinessScore,
    DateTimeOffset? LastForecastedAt,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<ReadinessForecastResponse> Forecasts);

public sealed record EntityExportFormatDescriptor(
    string FormatKey,
    string ContentType,
    string FileNameTemplate,
    string Description);

public sealed record EntityExportManifestEntity(
    string EntityKey,
    string ExportPath,
    string DisplayName,
    string CsvHeader,
    string Description,
    IReadOnlyList<EntityExportFormatDescriptor> Formats);

public sealed record EntityExportReportDescriptor(
    string ReportKey,
    string ExportPath,
    string DisplayName,
    string Description);

public sealed record EntityExportManifestResponse(
    string PackageVersion,
    IReadOnlyList<EntityExportManifestEntity> Entities,
    IReadOnlyList<EntityExportReportDescriptor> ReportExports,
    IReadOnlyList<string> AuditPackageFormats);

public sealed record CsvExportResult(string ContentType, string FileName, byte[] Content);
