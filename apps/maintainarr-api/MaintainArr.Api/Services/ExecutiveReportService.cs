using System.Text;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
namespace MaintainArr.Api.Services;

public sealed class ExecutiveReportService(
    MaintainArrDbContext db,
    AssetDowntimeService downtimeService,
    DowntimeTrackingSettingsService downtimeSettingsService)
{
    private static readonly IReadOnlySet<string> OpenDefectStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        DefectStatuses.Open,
        DefectStatuses.Acknowledged,
        DefectStatuses.InRepair,
    };

    private static readonly IReadOnlySet<string> OpenProcurementStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement,
        WorkOrderPartsDemandProcurementStatuses.PrDrafted,
        WorkOrderPartsDemandProcurementStatuses.PrSubmitted,
        WorkOrderPartsDemandProcurementStatuses.PrApproved,
        WorkOrderPartsDemandProcurementStatuses.PoCreated,
        WorkOrderPartsDemandProcurementStatuses.PoIssued,
        WorkOrderPartsDemandProcurementStatuses.PartiallyReceived,
    };

    public async Task<ExecutiveReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var generatedAt = DateTimeOffset.UtcNow;
        var fleetReadiness = await BuildFleetReadinessAsync(tenantId, cancellationToken);
        var operationalTotals = await BuildOperationalTotalsAsync(tenantId, generatedAt, cancellationToken);
        var downtimeTrend = await BuildDowntimeTrendAsync(tenantId, generatedAt, cancellationToken);
        var supplyDemand = await BuildSupplyDemandSummaryAsync(tenantId, cancellationToken);
        var scopeReadiness = await BuildScopeReadinessAsync(tenantId, cancellationToken);

        var workOrderStatusCounts = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var defectSeverityCounts = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && OpenDefectStatuses.Contains(x.Status))
            .GroupBy(x => x.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new ExecutiveReportSummaryResponse(
            generatedAt,
            fleetReadiness,
            operationalTotals,
            downtimeTrend,
            supplyDemand,
            scopeReadiness,
            workOrderStatusCounts
                .OrderBy(x => x.Status)
                .Select(x => new ExecutiveReportCountItem(x.Status, x.Count))
                .ToList(),
            defectSeverityCounts
                .OrderByDescending(x => x.Count)
                .Select(x => new ExecutiveReportCountItem(x.Severity, x.Count))
                .ToList());
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("section,key,value");
        builder.AppendLine($"fleet,total_assets,{summary.FleetReadiness.TotalAssets}");
        builder.AppendLine($"fleet,ready_count,{summary.FleetReadiness.ReadyCount}");
        builder.AppendLine($"fleet,not_ready_count,{summary.FleetReadiness.NotReadyCount}");
        builder.AppendLine($"fleet,ready_percent,{summary.FleetReadiness.ReadyPercent:F2}");
        builder.AppendLine($"operations,open_work_orders,{summary.OperationalTotals.OpenWorkOrderCount}");
        builder.AppendLine($"operations,open_critical_defects,{summary.OperationalTotals.OpenCriticalDefectCount}");
        builder.AppendLine($"operations,open_high_defects,{summary.OperationalTotals.OpenHighDefectCount}");
        builder.AppendLine($"operations,overdue_pm,{summary.OperationalTotals.OverduePmScheduleCount}");
        builder.AppendLine($"operations,labor_hours_30d,{summary.OperationalTotals.LaborHoursLast30Days:F2}");
        builder.AppendLine($"downtime,period_days,{summary.DowntimeTrend.PeriodDays}");
        builder.AppendLine($"downtime,current_hours,{summary.DowntimeTrend.CurrentPeriod.DowntimeHours:F2}");
        builder.AppendLine($"downtime,previous_hours,{summary.DowntimeTrend.PreviousPeriod.DowntimeHours:F2}");
        builder.AppendLine($"downtime,hours_delta,{summary.DowntimeTrend.DowntimeHoursDelta:F2}");
        builder.AppendLine($"downtime,current_availability_percent,{summary.DowntimeTrend.CurrentPeriod.AvailabilityPercent:F2}");
        builder.AppendLine($"downtime,availability_percent_delta,{summary.DowntimeTrend.AvailabilityPercentDelta:F2}");
        builder.AppendLine($"supplyarr,published_demand_lines,{summary.SupplyDemand.PublishedDemandLines}");
        builder.AppendLine($"supplyarr,open_procurement_lines,{summary.SupplyDemand.OpenProcurementLines}");

        builder.AppendLine();
        builder.AppendLine("scope_type,scope_label,total_assets,ready_count,not_ready_count,ready_percent");
        foreach (var scope in summary.ScopeReadiness)
        {
            builder.AppendLine(
                $"{CsvEscape(scope.ScopeType)},{CsvEscape(scope.ScopeLabel)},{scope.TotalAssets},{scope.ReadyCount},{scope.NotReadyCount},{scope.ReadyPercent:F2}");
        }

        var content = Encoding.UTF8.GetBytes(builder.ToString());
        return (
            "text/csv",
            $"maintainarr-executive-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            content);
    }

    private async Task<ExecutiveReportFleetReadiness> BuildFleetReadinessAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var fleetScope = await db.AssetStatusScopeRollups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ScopeType == AssetStatusRollupScopeTypes.Fleet
                    && x.ScopeEntityId == tenantId,
                cancellationToken);

        if (fleetScope is not null)
        {
            return new ExecutiveReportFleetReadiness(
                fleetScope.TotalAssets,
                fleetScope.ReadyCount,
                fleetScope.NotReadyCount,
                fleetScope.ReadyPercent,
                fleetScope.ComputedAt,
                FromScopeRollup: true);
        }

        var rollups = await db.AssetStatusRollups
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.ReadinessStatus)
            .ToListAsync(cancellationToken);

        if (rollups.Count == 0)
        {
            return new ExecutiveReportFleetReadiness(0, 0, 0, 0m, null, FromScopeRollup: false);
        }

        var readyCount = rollups.Count(x =>
            string.Equals(x, "ready", StringComparison.OrdinalIgnoreCase));
        var total = rollups.Count;
        var notReady = total - readyCount;
        var percent = total == 0 ? 0m : Math.Round((decimal)readyCount / total * 100m, 2);

        return new ExecutiveReportFleetReadiness(total, readyCount, notReady, percent, null, FromScopeRollup: false);
    }

    private async Task<ExecutiveReportOperationalTotals> BuildOperationalTotalsAsync(
        Guid tenantId,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = generatedAt.AddDays(-30);

        var assetCounts = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(x => x.LifecycleStatus == "active"),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var rollupTotals = await db.AssetStatusRollups
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                OpenCritical = g.Sum(x => x.OpenCriticalDefectCount),
                OpenHigh = g.Sum(x => x.OpenHighDefectCount),
                ActiveWorkOrders = g.Sum(x => x.ActiveWorkOrderCount),
                PmOverdue = g.Sum(x => x.PmOverdueCount),
                FailedInspections = g.Sum(x => x.FailedInspectionCount),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var openWorkOrders = rollupTotals?.ActiveWorkOrders
            ?? await db.WorkOrders
                .AsNoTracking()
                .CountAsync(
                    x => x.TenantId == tenantId && WorkOrderStatuses.Active.Contains(x.Status),
                    cancellationToken);

        var laborHours = await db.WorkOrderLaborEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.LoggedAt >= thirtyDaysAgo)
            .SumAsync(x => (decimal?)x.HoursWorked, cancellationToken) ?? 0m;

        var completedWorkOrders = await db.WorkOrders
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId
                    && x.Status == WorkOrderStatuses.Completed
                    && x.CompletedAt >= thirtyDaysAgo,
                cancellationToken);

        var activeTechnicians = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && WorkOrderStatuses.Active.Contains(x.Status)
                && x.AssignedTechnicianPersonId != null)
            .Select(x => x.AssignedTechnicianPersonId!)
            .Distinct()
            .CountAsync(cancellationToken);

        return new ExecutiveReportOperationalTotals(
            assetCounts?.Total ?? 0,
            assetCounts?.Active ?? 0,
            openWorkOrders,
            rollupTotals?.OpenCritical ?? 0,
            rollupTotals?.OpenHigh ?? 0,
            rollupTotals?.PmOverdue ?? 0,
            rollupTotals?.FailedInspections ?? 0,
            laborHours,
            completedWorkOrders,
            activeTechnicians);
    }

    private async Task<ExecutiveReportDowntimeTrend> BuildDowntimeTrendAsync(
        Guid tenantId,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken)
    {
        var settings = await downtimeSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var periodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(settings?.AvailabilityPeriodDays);
        var currentEnd = generatedAt;
        var currentStart = currentEnd.AddDays(-periodDays);
        var previousEnd = currentStart;
        var previousStart = previousEnd.AddDays(-periodDays);

        var currentFleet = await downtimeService.GetFleetAvailabilityAsync(
            tenantId,
            currentStart,
            currentEnd,
            cancellationToken);
        DateTimeOffset? snapshotComputedAt = currentFleet.IsMaterialized ? currentFleet.ComputedAt : null;

        var previousFleet = await downtimeService.ComputeFleetAvailabilityAsync(
            tenantId,
            previousStart,
            previousEnd,
            cancellationToken);

        var currentPeriod = MapDowntimePeriod(currentFleet);
        var previousPeriod = MapDowntimePeriod(previousFleet);

        return new ExecutiveReportDowntimeTrend(
            periodDays,
            currentPeriod,
            previousPeriod,
            Math.Round(currentPeriod.DowntimeHours - previousPeriod.DowntimeHours, 2),
            Math.Round(currentPeriod.AvailabilityPercent - previousPeriod.AvailabilityPercent, 1),
            snapshotComputedAt);
    }

    private static ExecutiveReportDowntimePeriodMetrics MapDowntimePeriod(FleetAvailabilityResponse fleet) =>
        new(
            fleet.PeriodStart,
            fleet.PeriodEnd,
            fleet.DowntimeHours,
            fleet.AvailabilityPercent,
            fleet.PlannedDowntimeHours,
            fleet.UnplannedDowntimeHours,
            fleet.ActiveDowntimeEventCount,
            fleet.IsMaterialized);

    private async Task<ExecutiveReportSupplyDemandSummary> BuildSupplyDemandSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var lines = await db.WorkOrderPartsDemandLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status != WorkOrderPartsDemandStatuses.Cancelled)
            .Select(x => new { x.SupplyarrDemandRefId, x.ProcurementStatus })
            .ToListAsync(cancellationToken);

        var published = lines.Count(x => x.SupplyarrDemandRefId is not null);
        var openProcurement = lines.Count(x => OpenProcurementStatuses.Contains(x.ProcurementStatus));
        var fulfilled = lines.Count(x =>
            string.Equals(x.ProcurementStatus, WorkOrderPartsDemandProcurementStatuses.Fulfilled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(x.ProcurementStatus, WorkOrderPartsDemandProcurementStatuses.ReceivedComplete, StringComparison.OrdinalIgnoreCase));

        var procurementCounts = lines
            .GroupBy(x => x.ProcurementStatus)
            .OrderByDescending(g => g.Count())
            .Select(g => new ExecutiveReportCountItem(g.Key, g.Count()))
            .ToList();

        return new ExecutiveReportSupplyDemandSummary(
            "supplyarr",
            lines.Count,
            published,
            openProcurement,
            fulfilled,
            procurementCounts);
    }

    private async Task<IReadOnlyList<ExecutiveReportScopeReadinessItem>> BuildScopeReadinessAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var scopes = await db.AssetStatusScopeRollups
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType != AssetStatusRollupScopeTypes.Fleet)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.ScopeLabel)
            .ToListAsync(cancellationToken);

        return scopes
            .Select(x => new ExecutiveReportScopeReadinessItem(
                x.ScopeType,
                x.ScopeEntityId,
                x.ScopeLabel,
                x.TotalAssets,
                x.ReadyCount,
                x.NotReadyCount,
                x.ReadyPercent,
                x.ComputedAt))
            .ToList();
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
