namespace TrainArr.Api.Contracts;

public sealed record AssignmentReportSummaryResponse(
    int TotalAssignments,
    int OpenAssignments,
    int CompletedAssignments,
    int OverdueAssignments,
    decimal CompletionRatePercent,
    IReadOnlyList<AssignmentReportSummaryItem> RecentAssignments);

public sealed record AssignmentReportSummaryItem(
    Guid AssignmentId,
    Guid StaffarrPersonId,
    string DefinitionKey,
    string DefinitionName,
    string Status,
    DateTimeOffset? DueAt,
    bool IsOverdue,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record QualificationReportSummaryResponse(
    int TotalQualifications,
    int IssuedCount,
    int ExpiredCount,
    int SuspendedCount,
    int RevokedCount,
    int ExpiringWithin30Days,
    IReadOnlyList<QualificationReportSummaryItem> RecentQualifications);

public sealed record QualificationReportSummaryItem(
    Guid QualificationIssueId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    bool ExpiringSoon);

public sealed record ComplianceReportSummaryResponse(
    int CitationAttachmentCount,
    int RulePackRequirementCount,
    int OpenRemediationCount,
    int TotalRemediationCount,
    int AttentionItemCount,
    IReadOnlyList<ComplianceReportRemediationItem> RecentRemediations);

public sealed record ComplianceReportRemediationItem(
    Guid RemediationId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

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
