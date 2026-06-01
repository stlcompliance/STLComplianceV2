using System.Text;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceReportService(MaintainArrDbContext db)
{
    private const int DetailListLimit = 25;

    private static readonly string[] OpenDefectStatuses =
    [
        DefectStatuses.Open,
        DefectStatuses.Acknowledged,
        DefectStatuses.InRepair,
    ];

    public async Task<MaintenanceReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? lifecycleStatus,
        CancellationToken cancellationToken = default)
    {
        var assetsQuery = db.Assets.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(lifecycleStatus))
        {
            var normalized = lifecycleStatus.Trim().ToLowerInvariant();
            assetsQuery = assetsQuery.Where(x => x.LifecycleStatus == normalized);
        }

        var assets = await assetsQuery
            .OrderBy(x => x.AssetTag)
            .ToListAsync(cancellationToken);

        var assetIds = assets.Select(x => x.Id).ToList();
        if (assetIds.Count == 0)
        {
            return EmptySummary();
        }

        var rollups = await db.AssetStatusRollups
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);
        var rollupByAsset = rollups.ToDictionary(x => x.AssetId);

        var workOrders = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .Select(x => new { x.AssetId, x.Status, x.CompletedAt })
            .ToListAsync(cancellationToken);

        var defects = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .Select(x => new { x.AssetId, x.Status, x.Severity })
            .ToListAsync(cancellationToken);

        var inspections = await db.InspectionRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .Select(x => new { x.AssetId, x.Status, x.CompletedAt })
            .ToListAsync(cancellationToken);

        var pmSchedules = await db.PmSchedules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .Select(x => new { x.AssetId, x.DueStatus })
            .ToListAsync(cancellationToken);

        var assetItems = assets.Select(asset =>
        {
            rollupByAsset.TryGetValue(asset.Id, out var rollup);
            var assetWorkOrders = workOrders.Where(x => x.AssetId == asset.Id).ToList();
            var assetDefects = defects.Where(x => x.AssetId == asset.Id).ToList();
            var assetInspections = inspections.Where(x => x.AssetId == asset.Id).ToList();
            var assetPm = pmSchedules.Where(x => x.AssetId == asset.Id).ToList();

            return new MaintenanceReportAssetSummaryItem(
                asset.Id,
                asset.AssetTag,
                asset.Name,
                asset.LifecycleStatus,
                asset.SiteRef,
                rollup?.ReadinessStatus,
                assetWorkOrders.Count(x => WorkOrderStatuses.Active.Contains(x.Status)),
                assetDefects.Count(x => OpenDefectStatuses.Contains(x.Status)),
                assetPm.Count(x => string.Equals(x.DueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase)),
                assetPm.Count(x => string.Equals(x.DueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)),
                assetInspections
                    .Where(x => x.CompletedAt is not null)
                    .MaxBy(x => x.CompletedAt)?.CompletedAt,
                assetWorkOrders
                    .Where(x => x.CompletedAt is not null)
                    .MaxBy(x => x.CompletedAt)?.CompletedAt);
        }).ToList();

        return new MaintenanceReportSummaryResponse(
            DateTimeOffset.UtcNow,
            assets.Count,
            assets.Count(x => string.Equals(x.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase)),
            CountBy(workOrders.Select(x => x.Status)),
            CountBy(defects.Select(x => x.Status)),
            CountBy(defects.Select(x => x.Severity)),
            CountBy(inspections.Select(x => x.Status)),
            CountBy(pmSchedules.Select(x => x.DueStatus)),
            CountBy(rollups.Select(x => x.ReadinessStatus)),
            assetItems);
    }

    public async Task<MaintenanceReportAssetDetailResponse> GetAssetDetailAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var summary = await BuildAssetSummaryAsync(tenantId, assetId, cancellationToken);

        var recentWorkOrders = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new MaintenanceReportWorkOrderRow(
                x.Id,
                x.WorkOrderNumber,
                x.Title,
                x.Status,
                x.Priority,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var openDefects = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.AssetId == assetId
                && OpenDefectStatuses.Contains(x.Status))
            .OrderByDescending(x => x.CreatedAt)
            .Take(DetailListLimit)
            .Select(x => new MaintenanceReportDefectRow(
                x.Id,
                x.Title,
                x.Severity,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var recentInspectionRuns = await (
            from run in db.InspectionRuns.AsNoTracking()
            join template in db.InspectionTemplates.AsNoTracking()
                on run.InspectionTemplateId equals template.Id
            where run.TenantId == tenantId && run.AssetId == assetId
            orderby run.StartedAt descending
            select new MaintenanceReportInspectionRunRow(
                run.Id,
                template.Name,
                run.Status,
                run.Result,
                run.StartedAt,
                run.CompletedAt))
            .Take(DetailListLimit)
            .ToListAsync(cancellationToken);

        var pmSchedules = await db.PmSchedules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderBy(x => x.NextDueAt)
            .Take(DetailListLimit)
            .Select(x => new MaintenanceReportPmScheduleRow(
                x.Id,
                x.ScheduleKey,
                x.Name,
                x.DueStatus,
                x.NextDueAt,
                x.LastCompletedAt))
            .ToListAsync(cancellationToken);

        return new MaintenanceReportAssetDetailResponse(
            summary,
            recentWorkOrders,
            openDefects,
            recentInspectionRuns,
            pmSchedules);
    }

    public async Task<MaintenanceReportWorkOrderDetailResponse> GetWorkOrderDetailAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken)
            ?? throw new StlApiException("reports.work_order_not_found", "Work order was not found.", 404);

        var taskLineCount = await db.WorkOrderTaskLines
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId, cancellationToken);

        var evidenceCount = await db.WorkOrderEvidence
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId, cancellationToken);

        var totalLaborHours = await db.WorkOrderLaborEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .SumAsync(x => x.HoursWorked, cancellationToken);

        return new MaintenanceReportWorkOrderDetailResponse(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.Title,
            workOrder.Description,
            workOrder.Status,
            workOrder.Priority,
            workOrder.Source,
            workOrder.AssetId,
            workOrder.Asset.AssetTag,
            workOrder.Asset.Name,
            workOrder.DefectId,
            workOrder.PmScheduleId,
            workOrder.AssignedTechnicianPersonId,
            taskLineCount,
            evidenceCount,
            totalLaborHours,
            workOrder.CreatedAt,
            workOrder.UpdatedAt,
            workOrder.StartedAt,
            workOrder.CompletedAt);
    }

    public async Task<MaintenanceReportDefectDetailResponse> GetDefectDetailAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken = default)
    {
        var defect = await db.Defects
            .AsNoTracking()
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defectId, cancellationToken)
            ?? throw new StlApiException("reports.defect_not_found", "Defect was not found.", 404);

        var linkedWorkOrderCount = await db.WorkOrders
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.DefectId == defectId, cancellationToken);

        return new MaintenanceReportDefectDetailResponse(
            defect.Id,
            defect.Title,
            defect.Description,
            defect.Severity,
            defect.Status,
            defect.Source,
            defect.AssetId,
            defect.Asset.AssetTag,
            defect.Asset.Name,
            defect.InspectionRunId,
            linkedWorkOrderCount,
            defect.CreatedAt,
            defect.UpdatedAt,
            defect.ResolvedAt);
    }

    public async Task<MaintenanceReportInspectionRunDetailResponse> GetInspectionRunDetailAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await db.InspectionRuns
            .AsNoTracking()
            .Include(x => x.Asset)
            .Include(x => x.InspectionTemplate)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inspectionRunId, cancellationToken)
            ?? throw new StlApiException("reports.inspection_run_not_found", "Inspection run was not found.", 404);

        var answers = await db.InspectionRunAnswers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId)
            .Select(x => x.PassFailValue)
            .ToListAsync(cancellationToken);

        var linkedDefectCount = await db.Defects
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId, cancellationToken);

        return new MaintenanceReportInspectionRunDetailResponse(
            run.Id,
            run.AssetId,
            run.Asset.AssetTag,
            run.Asset.Name,
            run.InspectionTemplate.Name,
            run.TemplateVersion,
            run.Status,
            run.Result,
            answers.Count,
            answers.Count(x => string.Equals(x, InspectionAnswerPassFailValues.Fail, StringComparison.OrdinalIgnoreCase)),
            linkedDefectCount,
            run.StartedAt,
            run.CompletedAt);
    }

    public async Task<MaintenanceReportPmScheduleDetailResponse> GetPmScheduleDetailAsync(
        Guid tenantId,
        Guid pmScheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await db.PmSchedules
            .AsNoTracking()
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == pmScheduleId, cancellationToken)
            ?? throw new StlApiException("reports.pm_schedule_not_found", "PM schedule was not found.", 404);

        var linkedWorkOrderCount = await db.WorkOrders
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.PmScheduleId == pmScheduleId, cancellationToken);

        return new MaintenanceReportPmScheduleDetailResponse(
            schedule.Id,
            schedule.AssetId,
            schedule.Asset.AssetTag,
            schedule.Asset.Name,
            schedule.ScheduleKey,
            schedule.Name,
            schedule.Description,
            schedule.ScheduleMode,
            schedule.DueStatus,
            schedule.Status,
            schedule.IntervalDays,
            schedule.NextDueAt,
            schedule.LastCompletedAt,
            linkedWorkOrderCount);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? lifecycleStatus,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, lifecycleStatus, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "assetTag,assetName,lifecycleStatus,siteRef,readinessStatus,openWorkOrderCount,openDefectCount,overduePmScheduleCount,duePmScheduleCount,lastInspectionCompletedAt,lastWorkOrderCompletedAt");

        foreach (var asset in summary.Assets)
        {
            builder.Append(CsvEscape(asset.AssetTag));
            builder.Append(',');
            builder.Append(CsvEscape(asset.AssetName));
            builder.Append(',');
            builder.Append(CsvEscape(asset.LifecycleStatus));
            builder.Append(',');
            builder.Append(CsvEscape(asset.SiteRef ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(asset.ReadinessStatus ?? string.Empty));
            builder.Append(',');
            builder.Append(asset.OpenWorkOrderCount);
            builder.Append(',');
            builder.Append(asset.OpenDefectCount);
            builder.Append(',');
            builder.Append(asset.OverduePmScheduleCount);
            builder.Append(',');
            builder.Append(asset.DuePmScheduleCount);
            builder.Append(',');
            builder.Append(asset.LastInspectionCompletedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(asset.LastWorkOrderCompletedAt?.ToString("O") ?? string.Empty);
        }

        var fileName = $"maintainarr-maintenance-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private async Task<MaintenanceReportAssetSummaryItem> BuildAssetSummaryAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var summary = await GetSummaryAsync(tenantId, null, cancellationToken);
        var asset = summary.Assets.FirstOrDefault(x => x.AssetId == assetId);
        if (asset is null)
        {
            throw new StlApiException("reports.asset_not_found", "Asset was not found.", 404);
        }

        return asset;
    }

    private static MaintenanceReportSummaryResponse EmptySummary() =>
        new(
            DateTimeOffset.UtcNow,
            0,
            0,
            [],
            [],
            [],
            [],
            [],
            [],
            []);

    private static IReadOnlyList<MaintenanceReportCountItem> CountBy(IEnumerable<string> keys) =>
        keys
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(g => new MaintenanceReportCountItem(g.Key, g.Count()))
            .OrderBy(x => x.Key)
            .ToList();

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
