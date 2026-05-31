namespace MaintainArr.Api.Contracts;

public sealed record MaintainArrDashboardResponse(
    DateTimeOffset GeneratedAt,
    FleetDashboardReadiness Readiness,
    FleetDashboardOperations Operations,
    FleetDashboardDowntime Downtime,
    FleetDashboardSupply Supply,
    IReadOnlyList<FleetDashboardActionItem> ActionItems);

public sealed record FleetDashboardReadiness(
    int TotalAssets,
    int ReadyAssets,
    int NotReadyAssets,
    decimal ReadyPercent,
    DateTimeOffset? SnapshotComputedAt);

public sealed record FleetDashboardOperations(
    int OpenWorkOrders,
    int CriticalDefects,
    int HighDefects,
    int OverduePm,
    int FailedInspections,
    int ActiveTechnicianAssignments,
    decimal LaborHoursLast30Days,
    int WorkOrdersCompletedLast30Days);

public sealed record FleetDashboardDowntime(
    int PeriodDays,
    decimal CurrentDowntimeHours,
    decimal AvailabilityPercent,
    decimal DowntimeHoursDelta,
    int ActiveDowntimeEvents,
    int ChronicAssetCount,
    IReadOnlyList<ExecutiveReportReliabilityAssetItem> TopProblemAssets);

public sealed record FleetDashboardSupply(
    int TotalDemandLines,
    int OpenProcurementLines,
    int FulfilledLines,
    int PublishedDemandLines);

public sealed record FleetDashboardActionItem(
    string Key,
    string Severity,
    int Count,
    string Message,
    string? Path);
