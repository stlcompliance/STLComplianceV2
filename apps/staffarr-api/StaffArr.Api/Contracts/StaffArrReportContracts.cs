namespace StaffArr.Api.Contracts;

public sealed record PersonnelReportSummaryResponse(
    int TotalPeople,
    int ActiveCount,
    int InactiveCount,
    int OnLeaveCount,
    decimal ActivePercent,
    IReadOnlyList<PersonnelReportSummaryItem> RecentPeople);

public sealed record PersonnelReportSummaryItem(
    Guid PersonId,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    string? PrimaryOrgUnitName,
    string? JobTitle,
    DateTimeOffset UpdatedAt);

public sealed record ReadinessReportSummaryResponse(
    int TotalRollups,
    int TotalMembers,
    int ReadyCount,
    int NotReadyCount,
    int OverrideCount,
    decimal ReadyPercent,
    IReadOnlyList<ReadinessReportSummaryItem> RecentRollups);

public sealed record ReadinessReportSummaryItem(
    Guid RollupId,
    string ScopeType,
    Guid OrgUnitId,
    string OrgUnitName,
    int TotalMembers,
    int ReadyCount,
    int NotReadyCount,
    decimal ReadyPercent,
    DateTimeOffset ComputedAt);

public sealed record ReadinessReportAlertResponse(
    string AlertType,
    string Severity,
    Guid PersonId,
    string PersonDisplayName,
    string Message,
    DateTimeOffset DetectedAt);

public sealed record CertificationReportSummaryResponse(
    int TotalPeople,
    int ActiveCertificationCount,
    int ExpiringSoonCount,
    int ExpiredCertificationCount,
    int MissingCertificationCount,
    IReadOnlyList<CertificationReportSummaryItem> RecentCertifications);

public sealed record CertificationReportSummaryItem(
    Guid PersonCertificationId,
    Guid PersonId,
    string PersonDisplayName,
    string CertificationKey,
    string CertificationName,
    string Status,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt);

public sealed record IncidentReportSummaryResponse(
    int TotalIncidents,
    int OpenCount,
    int ClosedCount,
    int HighSeverityOpenCount,
    IReadOnlyList<IncidentReportSummaryItem> RecentIncidents);

public sealed record IncidentReportSummaryItem(
    Guid IncidentId,
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Status,
    string Title,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt);

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
