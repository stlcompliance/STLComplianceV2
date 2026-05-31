using MaintainArr.Api.Contracts;

namespace MaintainArr.Api.Services;

public sealed class DashboardService(ExecutiveReportService executiveReportService)
{
    public async Task<MaintainArrDashboardResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var report = await executiveReportService.GetSummaryAsync(tenantId, cancellationToken);
        var operations = report.OperationalTotals;
        var downtime = report.DowntimeTrend.CurrentPeriod;

        return new MaintainArrDashboardResponse(
            report.GeneratedAt,
            new FleetDashboardReadiness(
                report.FleetReadiness.TotalAssets,
                report.FleetReadiness.ReadyCount,
                report.FleetReadiness.NotReadyCount,
                report.FleetReadiness.ReadyPercent,
                report.FleetReadiness.ComputedAt),
            new FleetDashboardOperations(
                operations.OpenWorkOrderCount,
                operations.OpenCriticalDefectCount,
                operations.OpenHighDefectCount,
                operations.OverduePmScheduleCount,
                operations.FailedInspectionCount,
                operations.ActiveTechnicianAssignments,
                operations.LaborHoursLast30Days,
                operations.WorkOrdersCompletedLast30Days),
            new FleetDashboardDowntime(
                report.DowntimeTrend.PeriodDays,
                downtime.DowntimeHours,
                downtime.AvailabilityPercent,
                report.DowntimeTrend.DowntimeHoursDelta,
                downtime.ActiveDowntimeEventCount,
                report.Reliability.ChronicAssetCount,
                report.Reliability.ChronicAssets),
            new FleetDashboardSupply(
                report.SupplyDemand.TotalDemandLines,
                report.SupplyDemand.OpenProcurementLines,
                report.SupplyDemand.FulfilledLines,
                report.SupplyDemand.PublishedDemandLines),
            BuildActionItems(report));
    }

    private static IReadOnlyList<FleetDashboardActionItem> BuildActionItems(ExecutiveReportSummaryResponse report)
    {
        List<FleetDashboardActionItem> items = [];
        var operations = report.OperationalTotals;

        AddItem(
            items,
            "asset_not_ready",
            "high",
            report.FleetReadiness.NotReadyCount,
            "Assets are not ready for operation.",
            "/api/v1/readiness");
        AddItem(
            items,
            "critical_defects",
            "critical",
            operations.OpenCriticalDefectCount,
            "Critical defects require immediate review.",
            "/api/v1/defects");
        AddItem(
            items,
            "high_defects",
            "high",
            operations.OpenHighDefectCount,
            "High severity defects are open.",
            "/api/v1/defects");
        AddItem(
            items,
            "overdue_pm",
            "high",
            operations.OverduePmScheduleCount,
            "Preventive maintenance is overdue.",
            "/api/v1/pm-events");
        AddItem(
            items,
            "failed_inspections",
            "high",
            operations.FailedInspectionCount,
            "Failed inspections need maintenance follow-up.",
            "/api/v1/inspection-runs");
        AddItem(
            items,
            "open_work_orders",
            "medium",
            operations.OpenWorkOrderCount,
            "Work orders are open.",
            "/api/v1/work-orders");
        AddItem(
            items,
            "waiting_on_parts",
            "medium",
            report.SupplyDemand.OpenProcurementLines,
            "Parts procurement is still open for work-order demand.",
            "/api/v1/work-orders");
        AddItem(
            items,
            "active_downtime",
            "high",
            report.DowntimeTrend.CurrentPeriod.ActiveDowntimeEventCount,
            "Assets currently have active downtime.",
            "/api/v1/downtime");
        AddItem(
            items,
            "chronic_assets",
            "medium",
            report.Reliability.ChronicAssetCount,
            "Assets show chronic downtime or reliability risk.",
            "/api/v1/reports/executive/summary");

        return items
            .OrderBy(x => SeveritySort(x.Severity))
            .ThenByDescending(x => x.Count)
            .ToList();
    }

    private static void AddItem(
        ICollection<FleetDashboardActionItem> items,
        string key,
        string severity,
        int count,
        string message,
        string path)
    {
        if (count <= 0)
        {
            return;
        }

        items.Add(new FleetDashboardActionItem(key, severity, count, message, path));
    }

    private static int SeveritySort(string severity) =>
        severity.ToLowerInvariant() switch
        {
            "critical" => 0,
            "high" => 1,
            "medium" => 2,
            _ => 3,
        };
}
