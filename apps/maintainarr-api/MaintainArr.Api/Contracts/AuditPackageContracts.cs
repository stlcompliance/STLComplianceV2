namespace MaintainArr.Api.Contracts;

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
    AuditPackageCountsResponse Counts,
    IReadOnlyList<AuditEventExportItem> AuditEvents,
    IReadOnlyList<AuditPackageAssetItem> Assets,
    IReadOnlyList<AuditPackageWorkOrderItem> WorkOrders,
    IReadOnlyList<AuditPackageDefectItem> Defects,
    IReadOnlyList<AuditPackageInspectionRunItem> InspectionRuns,
    IReadOnlyList<AuditPackagePmScheduleItem> PmSchedules);

public sealed record AuditPackageDateRangeResponse(
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record AuditPackageCountsResponse(
    int AuditEvents,
    int Assets,
    int WorkOrders,
    int Defects,
    int InspectionRuns,
    int PmSchedules);

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

public sealed record AuditPackageAssetItem(
    Guid AssetId,
    string AssetTag,
    string Name,
    string LifecycleStatus,
    string? SiteRef,
    string TypeKey,
    string ClassKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageWorkOrderItem(
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AssetId,
    string Title,
    string Status,
    string Priority,
    string Source,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record AuditPackageDefectItem(
    Guid DefectId,
    Guid AssetId,
    string Title,
    string Severity,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);

public sealed record AuditPackageInspectionRunItem(
    Guid InspectionRunId,
    Guid AssetId,
    string TemplateKey,
    string Status,
    string? Result,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record AuditPackagePmScheduleItem(
    Guid PmScheduleId,
    Guid AssetId,
    string ScheduleKey,
    string Name,
    string DueStatus,
    DateTimeOffset NextDueAt,
    DateTimeOffset? LastCompletedAt);
