using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceHistoryService(MaintainArrDbContext db)
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<MaintenanceHistoryEntryResponse>> ListAsync(
        Guid tenantId,
        Guid assetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var assetExists = await db.Assets.AnyAsync(
            x => x.TenantId == tenantId && x.Id == assetId,
            cancellationToken);
        if (!assetExists)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 50,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };

        var entries = await BuildTimelineEntriesAsync(tenantId, assetId, cancellationToken);
        var total = entries.Count;
        var items = entries
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.EntryId, StringComparer.Ordinal)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<MaintenanceHistoryEntryResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    private async Task<List<MaintenanceHistoryEntryResponse>> BuildTimelineEntriesAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var entries = new List<MaintenanceHistoryEntryResponse>();

        var inspectionRuns = await (
            from run in db.InspectionRuns.AsNoTracking()
            join template in db.InspectionTemplates.AsNoTracking()
                on run.InspectionTemplateId equals template.Id
            where run.TenantId == tenantId
                && run.AssetId == assetId
                && template.TenantId == tenantId
            select new
            {
                run.Id,
                run.AssetId,
                run.StartedAt,
                run.CompletedAt,
                run.StartedByUserId,
                run.Status,
                run.Result,
                TemplateName = template.Name,
                TemplateKey = template.TemplateKey,
            }).ToListAsync(cancellationToken);

        foreach (var run in inspectionRuns)
        {
            entries.Add(new MaintenanceHistoryEntryResponse(
                $"inspection:{run.Id}:started",
                run.AssetId,
                "inspection",
                "inspection_started",
                $"Inspection started: {run.TemplateName}",
                run.TemplateKey,
                run.StartedAt,
                run.StartedByUserId,
                "inspection_run",
                run.Id.ToString(),
                null));

            if (run.CompletedAt is DateTimeOffset completedAt)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"inspection:{run.Id}:completed",
                    run.AssetId,
                    "inspection",
                    "inspection_completed",
                    $"Inspection completed: {run.TemplateName}",
                    $"{run.Result ?? "unknown"} · {run.Status}",
                    completedAt,
                    run.StartedByUserId,
                    "inspection_run",
                    run.Id.ToString(),
                    null));
            }
        }

        var defects = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        foreach (var defect in defects)
        {
            entries.Add(new MaintenanceHistoryEntryResponse(
                $"defect:{defect.Id}:reported",
                defect.AssetId,
                "defect",
                "defect_reported",
                $"Defect reported: {defect.Title}",
                $"{defect.Severity} · {defect.Source} · {defect.Status}",
                defect.CreatedAt,
                defect.ReportedByUserId,
                "defect",
                defect.Id.ToString(),
                defect.InspectionRunId?.ToString()));

            if (defect.ResolvedAt is DateTimeOffset resolvedAt)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"defect:{defect.Id}:resolved",
                    defect.AssetId,
                    "defect",
                    "defect_resolved",
                    $"Defect resolved: {defect.Title}",
                    defect.Status,
                    resolvedAt,
                    defect.ReportedByUserId,
                    "defect",
                    defect.Id.ToString(),
                    defect.InspectionRunId?.ToString()));
            }
        }

        var workOrders = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        foreach (var workOrder in workOrders)
        {
            entries.Add(new MaintenanceHistoryEntryResponse(
                $"work_order:{workOrder.Id}:created",
                workOrder.AssetId,
                "work_order",
                "work_order_created",
                $"Work order created: {workOrder.Title}",
                $"{workOrder.WorkOrderNumber} · {workOrder.Priority} · {workOrder.Source}",
                workOrder.CreatedAt,
                workOrder.CreatedByUserId,
                "work_order",
                workOrder.Id.ToString(),
                workOrder.DefectId?.ToString() ?? workOrder.PmScheduleId?.ToString()));

            if (workOrder.StartedAt is DateTimeOffset startedAt)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"work_order:{workOrder.Id}:started",
                    workOrder.AssetId,
                    "work_order",
                    "work_order_started",
                    $"Work order started: {workOrder.Title}",
                    workOrder.WorkOrderNumber,
                    startedAt,
                    workOrder.CreatedByUserId,
                    "work_order",
                    workOrder.Id.ToString(),
                    null));
            }

            if (workOrder.CompletedAt is DateTimeOffset completedAt)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"work_order:{workOrder.Id}:completed",
                    workOrder.AssetId,
                    "work_order",
                    "work_order_completed",
                    $"Work order completed: {workOrder.Title}",
                    workOrder.WorkOrderNumber,
                    completedAt,
                    workOrder.CreatedByUserId,
                    "work_order",
                    workOrder.Id.ToString(),
                    null));
            }

            if (workOrder.CancelledAt is DateTimeOffset cancelledAt)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"work_order:{workOrder.Id}:cancelled",
                    workOrder.AssetId,
                    "work_order",
                    "work_order_cancelled",
                    $"Work order cancelled: {workOrder.Title}",
                    workOrder.WorkOrderNumber,
                    cancelledAt,
                    workOrder.CreatedByUserId,
                    "work_order",
                    workOrder.Id.ToString(),
                    null));
            }
        }

        var pmSchedules = await db.PmSchedules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        foreach (var schedule in pmSchedules)
        {
            entries.Add(new MaintenanceHistoryEntryResponse(
                $"pm:{schedule.Id}:created",
                schedule.AssetId,
                "pm",
                "pm_schedule_created",
                $"PM schedule created: {schedule.Name}",
                $"{schedule.ScheduleKey} · every {schedule.IntervalDays} days",
                schedule.CreatedAt,
                null,
                "pm_schedule",
                schedule.Id.ToString(),
                null));

            if (schedule.LastCompletedAt is DateTimeOffset lastCompletedAt)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"pm:{schedule.Id}:completed",
                    schedule.AssetId,
                    "pm",
                    "pm_completed",
                    $"PM completed: {schedule.Name}",
                    schedule.ScheduleKey,
                    lastCompletedAt,
                    null,
                    "pm_schedule",
                    schedule.Id.ToString(),
                    null));
            }

            if (schedule.LastDueScanAt is DateTimeOffset lastDueScanAt
                && string.Equals(schedule.DueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"pm:{schedule.Id}:due",
                    schedule.AssetId,
                    "pm",
                    "pm_marked_due",
                    $"PM due: {schedule.Name}",
                    $"Next due {schedule.NextDueAt:u}",
                    lastDueScanAt,
                    null,
                    "pm_schedule",
                    schedule.Id.ToString(),
                    null));
            }

            if (schedule.LastDueScanAt is DateTimeOffset overdueScanAt
                && string.Equals(schedule.DueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"pm:{schedule.Id}:overdue",
                    schedule.AssetId,
                    "pm",
                    "pm_marked_overdue",
                    $"PM overdue: {schedule.Name}",
                    $"Next due {schedule.NextDueAt:u}",
                    overdueScanAt,
                    null,
                    "pm_schedule",
                    schedule.Id.ToString(),
                    null));
            }
        }

        return entries;
    }
}
