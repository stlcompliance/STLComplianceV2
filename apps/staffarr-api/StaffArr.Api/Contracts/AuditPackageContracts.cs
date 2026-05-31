namespace StaffArr.Api.Contracts;

public sealed record AuditPackageManifestResponse(
    string PackageVersion,
    IReadOnlyList<AuditPackageSectionDescriptor> Sections);

public sealed record AuditPackageSectionDescriptor(
    string Key,
    string FileName,
    string Label,
    string Description);

public sealed record AuditPackageExportResponse(
    Guid PackageId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    AuditPackageDateRangeResponse? DateRange,
    AuditPackageAppliedFiltersResponse? AppliedFilters,
    AuditPackageCountsResponse Counts,
    IReadOnlyList<StaffArrAuditEventExportItem> AuditEvents,
    IReadOnlyList<AuditPackagePersonItem> People,
    IReadOnlyList<AuditPackagePermissionHistoryItem> PermissionHistory,
    IReadOnlyList<AuditPackagePersonCertificationItem> PersonCertifications,
    IReadOnlyList<AuditPackagePersonnelIncidentItem> PersonnelIncidents,
    IReadOnlyList<AuditPackageReadinessOverrideItem> ReadinessOverrides,
    IReadOnlyList<AuditPackageTrainingBlockerItem> TrainingBlockers);

public sealed record AuditPackageDateRangeResponse(
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record AuditPackageCountsResponse(
    int AuditEvents,
    int People,
    int PermissionHistory,
    int PersonCertifications,
    int PersonnelIncidents,
    int ReadinessOverrides,
    int TrainingBlockers);

public sealed record StaffArrAuditEventExportItem(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record AuditPackagePersonItem(
    Guid PersonId,
    Guid? ExternalUserId,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    Guid? PrimaryOrgUnitId,
    Guid? ManagerPersonId,
    string? JobTitle,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackagePermissionHistoryItem(
    Guid PermissionHistoryEventId,
    Guid PersonId,
    Guid AssignmentId,
    Guid RoleTemplateId,
    Guid PermissionTemplateId,
    Guid? ActorUserId,
    string EventType,
    string AssignmentStatus,
    string RoleKey,
    string RoleName,
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    DateTimeOffset OccurredAt);

public sealed record AuditPackagePersonCertificationItem(
    Guid PersonCertificationId,
    Guid PersonId,
    Guid CertificationDefinitionId,
    string SourceType,
    string Status,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    string? Notes,
    Guid? GrantedByUserId,
    Guid? ExternalPublicationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackagePersonnelIncidentItem(
    Guid IncidentId,
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Status,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt,
    Guid ReportedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? SourceProduct = null,
    Guid? SourceIncidentId = null,
    string? SourceEventKind = null,
    string? SourceReferenceKey = null);

public sealed record AuditPackageReadinessOverrideItem(
    Guid ReadinessOverrideId,
    Guid PersonId,
    string Status,
    string Reason,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    Guid GrantedByUserId,
    DateTimeOffset? ClearedAt,
    Guid? ClearedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageTrainingBlockerItem(
    Guid TrainingBlockerId,
    Guid PersonId,
    Guid TrainarrPublicationId,
    string QualificationKey,
    string QualificationName,
    string BlockerType,
    string Message,
    string Status,
    DateTimeOffset PublishedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? ClearedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
