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

public sealed record ExecutiveReportDowntimePeriodMetrics(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal DowntimeHours,
    decimal AvailabilityPercent,
    decimal PlannedDowntimeHours,
    decimal UnplannedDowntimeHours,
    int ActiveDowntimeEventCount,
    bool FromMaterializedSnapshot);

public sealed record ExecutiveReportDowntimeTrend(
    int PeriodDays,
    ExecutiveReportDowntimePeriodMetrics CurrentPeriod,
    ExecutiveReportDowntimePeriodMetrics PreviousPeriod,
    decimal DowntimeHoursDelta,
    decimal AvailabilityPercentDelta,
    DateTimeOffset? FleetSnapshotComputedAt);

public sealed record ExecutiveReportReliabilityAssetItem(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    int DowntimeEventCount,
    decimal DowntimeHours,
    decimal AvailabilityPercent,
    bool HasActiveDowntime,
    DateTimeOffset? LastDowntimeStartedAt);

public sealed record ExecutiveReportReliabilitySummary(
    int PeriodDays,
    int ClosedRepairEventCount,
    int FailureEventCount,
    int RepeatDowntimeAssetCount,
    int ChronicAssetCount,
    decimal MeanTimeToRepairHours,
    decimal MeanTimeBetweenFailuresHours,
    IReadOnlyList<ExecutiveReportReliabilityAssetItem> ChronicAssets);

public sealed record ExecutiveReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    ExecutiveReportFleetReadiness FleetReadiness,
    ExecutiveReportOperationalTotals OperationalTotals,
    ExecutiveReportDowntimeTrend DowntimeTrend,
    ExecutiveReportReliabilitySummary Reliability,
    ExecutiveReportSupplyDemandSummary SupplyDemand,
    IReadOnlyList<ExecutiveReportScopeReadinessItem> ScopeReadiness,
    IReadOnlyList<ExecutiveReportCountItem> WorkOrderStatusCounts,
    IReadOnlyList<ExecutiveReportCountItem> DefectSeverityCounts);
