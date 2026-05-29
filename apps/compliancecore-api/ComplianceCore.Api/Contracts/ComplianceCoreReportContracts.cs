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
