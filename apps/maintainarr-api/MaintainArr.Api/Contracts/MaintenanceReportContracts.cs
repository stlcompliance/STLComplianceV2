namespace MaintainArr.Api.Contracts;

public sealed record MaintenanceReportCountItem(string Key, int Count);

public sealed record MaintenanceReportAssetSummaryItem(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string LifecycleStatus,
    string? SiteRef,
    string? ReadinessStatus,
    int OpenWorkOrderCount,
    int OpenDefectCount,
    int OverduePmScheduleCount,
    int DuePmScheduleCount,
    DateTimeOffset? LastInspectionCompletedAt,
    DateTimeOffset? LastWorkOrderCompletedAt);

public sealed record MaintenanceReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    int TotalAssetCount,
    int ActiveAssetCount,
    IReadOnlyList<MaintenanceReportCountItem> WorkOrderStatusCounts,
    IReadOnlyList<MaintenanceReportCountItem> DefectStatusCounts,
    IReadOnlyList<MaintenanceReportCountItem> DefectSeverityCounts,
    IReadOnlyList<MaintenanceReportCountItem> InspectionRunStatusCounts,
    IReadOnlyList<MaintenanceReportCountItem> PmDueStatusCounts,
    IReadOnlyList<MaintenanceReportCountItem> ReadinessStatusCounts,
    IReadOnlyList<MaintenanceReportAssetSummaryItem> Assets);

public sealed record MaintenanceReportWorkOrderRow(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string Title,
    string Status,
    string Priority,
    DateTimeOffset UpdatedAt);

public sealed record MaintenanceReportDefectRow(
    Guid DefectId,
    string Title,
    string Severity,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record MaintenanceReportInspectionRunRow(
    Guid InspectionRunId,
    string TemplateName,
    string Status,
    string? Result,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record MaintenanceReportPmScheduleRow(
    Guid PmScheduleId,
    string ScheduleKey,
    string Name,
    string DueStatus,
    DateTimeOffset NextDueAt,
    DateTimeOffset? LastCompletedAt);

public sealed record MaintenanceReportAssetDetailResponse(
    MaintenanceReportAssetSummaryItem Summary,
    IReadOnlyList<MaintenanceReportWorkOrderRow> RecentWorkOrders,
    IReadOnlyList<MaintenanceReportDefectRow> OpenDefects,
    IReadOnlyList<MaintenanceReportInspectionRunRow> RecentInspectionRuns,
    IReadOnlyList<MaintenanceReportPmScheduleRow> PmSchedules);

public sealed record MaintenanceReportWorkOrderDetailResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Source,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? DefectId,
    Guid? PmScheduleId,
    string? AssignedTechnicianPersonId,
    int TaskLineCount,
    int EvidenceCount,
    decimal TotalLaborHours,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record MaintenanceReportDefectDetailResponse(
    Guid DefectId,
    string Title,
    string Description,
    string Severity,
    string Status,
    string Source,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? InspectionRunId,
    int LinkedWorkOrderCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt);

public sealed record MaintenanceReportInspectionRunDetailResponse(
    Guid InspectionRunId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string TemplateName,
    int TemplateVersion,
    string Status,
    string? Result,
    int AnswerCount,
    int FailAnswerCount,
    int LinkedDefectCount,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record MaintenanceReportPmScheduleDetailResponse(
    Guid PmScheduleId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string ScheduleKey,
    string Name,
    string Description,
    string ScheduleMode,
    string DueStatus,
    string Status,
    int IntervalDays,
    DateTimeOffset NextDueAt,
    DateTimeOffset? LastCompletedAt,
    int LinkedWorkOrderCount);
