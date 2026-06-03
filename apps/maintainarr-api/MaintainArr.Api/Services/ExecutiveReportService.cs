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
    private static readonly string[] OpenDefectStatuses =
    [
        DefectStatuses.Open,
        DefectStatuses.Acknowledged,
        DefectStatuses.InRepair,
    ];

    private static readonly string[] OpenProcurementStatuses =
    [
        WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement,
        WorkOrderPartsDemandProcurementStatuses.PrDrafted,
        WorkOrderPartsDemandProcurementStatuses.PrSubmitted,
        WorkOrderPartsDemandProcurementStatuses.PrApproved,
        WorkOrderPartsDemandProcurementStatuses.PoCreated,
        WorkOrderPartsDemandProcurementStatuses.PoIssued,
        WorkOrderPartsDemandProcurementStatuses.PartiallyReceived,
    ];

    public async Task<ExecutiveReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var generatedAt = DateTimeOffset.UtcNow;
        var fleetReadiness = await BuildFleetReadinessAsync(tenantId, cancellationToken);
        var operationalTotals = await BuildOperationalTotalsAsync(tenantId, generatedAt, cancellationToken);
        var downtimeTrend = await BuildDowntimeTrendAsync(tenantId, generatedAt, cancellationToken);
        var reliability = await BuildReliabilitySummaryAsync(tenantId, generatedAt, cancellationToken);
        var supplyDemand = await BuildSupplyDemandSummaryAsync(tenantId, cancellationToken);
        var partsDemandForecast = await BuildPartsDemandForecastSummaryAsync(tenantId, cancellationToken);
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
            reliability,
            supplyDemand,
            partsDemandForecast,
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
        builder.AppendLine($"reliability,period_days,{summary.Reliability.PeriodDays}");
        builder.AppendLine($"reliability,closed_repair_events,{summary.Reliability.ClosedRepairEventCount}");
        builder.AppendLine($"reliability,failure_events,{summary.Reliability.FailureEventCount}");
        builder.AppendLine($"reliability,repeat_downtime_assets,{summary.Reliability.RepeatDowntimeAssetCount}");
        builder.AppendLine($"reliability,chronic_assets,{summary.Reliability.ChronicAssetCount}");
        builder.AppendLine($"reliability,mean_time_to_repair_hours,{summary.Reliability.MeanTimeToRepairHours:F2}");
        builder.AppendLine($"reliability,mean_time_between_failures_hours,{summary.Reliability.MeanTimeBetweenFailuresHours:F2}");
        builder.AppendLine($"supplyarr,published_demand_lines,{summary.SupplyDemand.PublishedDemandLines}");
        builder.AppendLine($"supplyarr,open_procurement_lines,{summary.SupplyDemand.OpenProcurementLines}");
        builder.AppendLine($"parts_demand_forecast,open_lines,{summary.PartsDemandForecast.OpenLineCount}");
        builder.AppendLine($"parts_demand_forecast,distinct_parts,{summary.PartsDemandForecast.DistinctPartCount}");
        builder.AppendLine($"parts_demand_forecast,forecast_quantity,{summary.PartsDemandForecast.ForecastQuantity:F2}");

        builder.AppendLine();
        builder.AppendLine("part_number,supplyarr_part_id,description,unit_of_measure,forecast_quantity,open_lines,open_work_orders,pm_work_orders,defect_work_orders,manual_work_orders,oldest_created_at,newest_created_at");
        foreach (var part in summary.PartsDemandForecast.TopParts)
        {
            builder.AppendLine(
                $"{CsvEscape(part.PartNumber)},{part.SupplyarrPartId?.ToString() ?? string.Empty},{CsvEscape(part.Description)},{CsvEscape(part.UnitOfMeasure)},{part.ForecastQuantity:F2},{part.OpenLineCount},{part.OpenWorkOrderCount},{part.PmWorkOrderCount},{part.DefectWorkOrderCount},{part.ManualWorkOrderCount},{part.OldestCreatedAt?.ToString("O") ?? string.Empty},{part.NewestCreatedAt?.ToString("O") ?? string.Empty}");
        }

        builder.AppendLine();
        builder.AppendLine("scope_type,scope_label,total_assets,ready_count,not_ready_count,ready_percent");
        foreach (var scope in summary.ScopeReadiness)
        {
            builder.AppendLine(
                $"{CsvEscape(scope.ScopeType)},{CsvEscape(scope.ScopeLabel)},{scope.TotalAssets},{scope.ReadyCount},{scope.NotReadyCount},{scope.ReadyPercent:F2}");
        }

        builder.AppendLine();
        builder.AppendLine("asset_id,asset_tag,asset_name,downtime_event_count,downtime_hours,availability_percent,has_active_downtime,last_downtime_started_at");
        foreach (var asset in summary.Reliability.ChronicAssets)
        {
            builder.AppendLine(
                $"{asset.AssetId},{CsvEscape(asset.AssetTag)},{CsvEscape(asset.AssetName)},{asset.DowntimeEventCount},{asset.DowntimeHours:F2},{asset.AvailabilityPercent:F2},{asset.HasActiveDowntime},{asset.LastDowntimeStartedAt?.ToString("O") ?? string.Empty}");
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

    private async Task<ExecutiveReportReliabilitySummary> BuildReliabilitySummaryAsync(
        Guid tenantId,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken)
    {
        var settings = await downtimeSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var periodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(settings?.AvailabilityPeriodDays);
        var periodEnd = generatedAt;
        var periodStart = periodEnd.AddDays(-periodDays);
        var totalHours = (decimal)(periodEnd - periodStart).TotalHours;

        var events = await db.AssetDowntimeEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.StartedAt < periodEnd
                && (x.EndedAt == null || x.EndedAt > periodStart))
            .ToListAsync(cancellationToken);

        var failureEvents = events
            .Where(x => !x.IsPlanned && x.StartedAt >= periodStart && x.StartedAt < periodEnd)
            .ToList();

        var closedRepairDurations = failureEvents
            .Where(x => x.EndedAt is not null)
            .Select(x => (decimal)(x.EndedAt!.Value - x.StartedAt).TotalHours)
            .Where(x => x >= 0m)
            .ToList();

        var failureGaps = failureEvents
            .GroupBy(x => x.AssetId)
            .SelectMany(g =>
            {
                var starts = g
                    .OrderBy(x => x.StartedAt)
                    .Select(x => x.StartedAt)
                    .ToList();
                return starts
                    .Skip(1)
                    .Select((startedAt, index) => (decimal)(startedAt - starts[index]).TotalHours);
            })
            .Where(x => x >= 0m)
            .ToList();

        var reliabilityAssets = events
            .GroupBy(x => x.AssetId)
            .Select(g =>
            {
                var intervals = g
                    .Select(x => new DowntimeInterval(x.StartedAt, x.EndedAt, x.IsPlanned))
                    .ToList();
                var downtimeHours = AssetDowntimeRules.ComputeDowntimeHoursForPeriod(intervals, periodStart, periodEnd);
                var eventCount = g.Count(x => x.StartedAt >= periodStart && x.StartedAt < periodEnd);
                var hasActiveDowntime = g.Any(x => x.EndedAt is null);
                return new ExecutiveReportReliabilityAssetItem(
                    g.Key,
                    g.OrderByDescending(x => x.StartedAt).First().AssetTag,
                    g.OrderByDescending(x => x.StartedAt).First().AssetName,
                    eventCount,
                    downtimeHours,
                    AssetDowntimeRules.ComputeAvailabilityPercent(totalHours, downtimeHours),
                    hasActiveDowntime,
                    g.Max(x => (DateTimeOffset?)x.StartedAt));
            })
            .Where(x => x.DowntimeEventCount >= 3
                || x.DowntimeHours >= 24m
                || (x.HasActiveDowntime && x.DowntimeEventCount >= 2)
                || x.AvailabilityPercent < 95m)
            .OrderByDescending(x => x.DowntimeHours)
            .ThenByDescending(x => x.DowntimeEventCount)
            .Take(10)
            .ToList();

        var repeatDowntimeAssetCount = failureEvents
            .GroupBy(x => x.AssetId)
            .Count(g => g.Count() >= 2);

        return new ExecutiveReportReliabilitySummary(
            periodDays,
            closedRepairDurations.Count,
            failureEvents.Count,
            repeatDowntimeAssetCount,
            reliabilityAssets.Count,
            closedRepairDurations.Count == 0 ? 0m : Math.Round(closedRepairDurations.Average(), 2),
            failureGaps.Count == 0 ? 0m : Math.Round(failureGaps.Average(), 2),
            reliabilityAssets);
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

    private async Task<ExecutiveReportPartsDemandForecastSummary> BuildPartsDemandForecastSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var demandLines = await (
            from line in db.WorkOrderPartsDemandLines.AsNoTracking()
            join workOrder in db.WorkOrders.AsNoTracking() on line.WorkOrderId equals workOrder.Id
            where line.TenantId == tenantId
                && workOrder.TenantId == tenantId
                && line.Status != WorkOrderPartsDemandStatuses.Cancelled
            select new
            {
                line.Id,
                line.SupplyarrPartId,
                line.PartNumber,
                line.Description,
                line.UnitOfMeasure,
                line.QuantityRequested,
                line.QuantityReceived,
                line.CreatedAt,
                WorkOrderId = workOrder.Id,
                workOrder.Source,
            })
            .ToListAsync(cancellationToken);

        var forecastRows = demandLines
            .Select(line =>
            {
                var outstanding = line.QuantityRequested - line.QuantityReceived;
                return new
                {
                    line.SupplyarrPartId,
                    PartNumber = string.IsNullOrWhiteSpace(line.PartNumber) ? "UNKNOWN" : line.PartNumber.Trim(),
                    Description = string.IsNullOrWhiteSpace(line.Description) ? string.Empty : line.Description.Trim(),
                    UnitOfMeasure = string.IsNullOrWhiteSpace(line.UnitOfMeasure) ? "each" : line.UnitOfMeasure.Trim(),
                    Outstanding = outstanding <= 0m ? 0m : outstanding,
                    line.WorkOrderId,
                    line.Source,
                    line.CreatedAt,
                };
            })
            .Where(x => x.Outstanding > 0m)
            .ToList();

        var items = forecastRows
            .GroupBy(x => new { x.SupplyarrPartId, x.PartNumber, x.UnitOfMeasure })
            .Select(group =>
            {
                var first = group.OrderBy(x => x.CreatedAt).First();
                return new ExecutiveReportPartsDemandForecastItem(
                    group.Key.SupplyarrPartId,
                    group.Key.PartNumber,
                    first.Description,
                    group.Key.UnitOfMeasure,
                    Math.Round(group.Sum(x => x.Outstanding), 2),
                    group.Count(),
                    group.Select(x => x.WorkOrderId).Distinct().Count(),
                    group.Where(x => string.Equals(x.Source, WorkOrderSources.PmSchedule, StringComparison.OrdinalIgnoreCase)).Select(x => x.WorkOrderId).Distinct().Count(),
                    group.Where(x => string.Equals(x.Source, WorkOrderSources.Defect, StringComparison.OrdinalIgnoreCase)).Select(x => x.WorkOrderId).Distinct().Count(),
                    group.Where(x => string.Equals(x.Source, WorkOrderSources.Manual, StringComparison.OrdinalIgnoreCase)).Select(x => x.WorkOrderId).Distinct().Count(),
                    group.Min(x => x.CreatedAt),
                    group.Max(x => x.CreatedAt));
            })
            .OrderByDescending(x => x.ForecastQuantity)
            .ThenByDescending(x => x.OpenLineCount)
            .ThenBy(x => x.PartNumber)
            .Take(10)
            .ToList();

        var distinctPartCount = forecastRows
            .GroupBy(x => new { x.SupplyarrPartId, x.PartNumber, x.UnitOfMeasure })
            .Count();

        return new ExecutiveReportPartsDemandForecastSummary(
            forecastRows.Count,
            distinctPartCount,
            Math.Round(forecastRows.Sum(x => x.Outstanding), 2),
            items);
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
