namespace MaintainArr.Api.Contracts;

public sealed record ExecutiveReportCountItem(string Key, int Count);

public sealed record ExecutiveReportFleetReadiness(
    int TotalAssets,
    int ReadyCount,
    int NotReadyCount,
    decimal ReadyPercent,
    DateTimeOffset? ComputedAt,
    bool FromScopeRollup);

public sealed record ExecutiveReportScopeReadinessItem(
    string ScopeType,
    Guid ScopeEntityId,
    string ScopeLabel,
    int TotalAssets,
    int ReadyCount,
    int NotReadyCount,
    decimal ReadyPercent,
    DateTimeOffset ComputedAt);

public sealed record ExecutiveReportSupplyDemandSummary(
    string SourceProduct,
    int TotalDemandLines,
    int PublishedDemandLines,
    int OpenProcurementLines,
    int FulfilledLines,
    IReadOnlyList<ExecutiveReportCountItem> ProcurementStatusCounts);

public sealed record ExecutiveReportOperationalTotals(
    int TotalAssetCount,
    int ActiveAssetCount,
    int OpenWorkOrderCount,
    int OpenCriticalDefectCount,
    int OpenHighDefectCount,
    int OverduePmScheduleCount,
    int FailedInspectionCount,
    decimal LaborHoursLast30Days,
    int WorkOrdersCompletedLast30Days,
    int ActiveTechnicianAssignments);

public sealed record ExecutiveReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    ExecutiveReportFleetReadiness FleetReadiness,
    ExecutiveReportOperationalTotals OperationalTotals,
    ExecutiveReportSupplyDemandSummary SupplyDemand,
    IReadOnlyList<ExecutiveReportScopeReadinessItem> ScopeReadiness,
    IReadOnlyList<ExecutiveReportCountItem> WorkOrderStatusCounts,
    IReadOnlyList<ExecutiveReportCountItem> DefectSeverityCounts);
