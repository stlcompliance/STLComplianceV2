namespace RoutArr.Api.Contracts;

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
    IReadOnlyList<AuditEventExportItem> AuditEvents,
    IReadOnlyList<ProofRecordExportItem> ProofRecords,
    IReadOnlyList<DvirInspectionExportItem> DvirInspections,
    IReadOnlyList<CaptureAttachmentExportItem> CaptureAttachments);

public sealed record AuditPackageDateRangeResponse(
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record AuditPackageCountsResponse(
    int AuditEvents,
    int ProofRecords,
    int DvirInspections,
    int CaptureAttachments);

public sealed record AuditEventExportItem(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record ProofRecordExportItem(
    Guid ProofId,
    Guid TripId,
    string TripNumber,
    string ProofType,
    string CapturedByPersonId,
    string? VehicleRefKey,
    string ReferenceKey,
    string Notes,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string EvidenceHash);

public sealed record DvirInspectionExportItem(
    Guid DvirId,
    Guid TripId,
    string TripNumber,
    string Phase,
    string VehicleRefKey,
    string Result,
    long? OdometerReading,
    string DefectNotes,
    string SubmittedByPersonId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string EvidenceHash);

public sealed record CaptureAttachmentExportItem(
    Guid AttachmentId,
    Guid TripId,
    string TripNumber,
    string SubjectType,
    Guid SubjectId,
    string AttachmentKind,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    string? Notes,
    string CapturedByPersonId,
    DateTimeOffset CreatedAt,
    string EvidenceHash);
