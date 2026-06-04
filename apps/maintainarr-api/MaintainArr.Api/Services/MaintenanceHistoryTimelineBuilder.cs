using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class MaintenanceHistoryTimelineBuilder
{
    public static async Task<List<MaintenanceHistoryEntryResponse>> BuildTimelineEntriesAsync(
        MaintainArrDbContext db,
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

            if (string.Equals(defect.Status, DefectStatuses.InRepair, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"defect:{defect.Id}:in_repair",
                    defect.AssetId,
                    "defect",
                    "defect_in_repair",
                    $"Defect in repair: {defect.Title}",
                    $"{defect.Severity} · {defect.Source} · {defect.Status}",
                    defect.UpdatedAt,
                    defect.ReportedByUserId,
                    "defect",
                    defect.Id.ToString(),
                    defect.InspectionRunId?.ToString()));
            }

            var linkedWorkOrders = await db.WorkOrders
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.DefectId == defect.Id)
                .ToListAsync(cancellationToken);

            foreach (var workOrder in linkedWorkOrders)
            {
                if (workOrder.StartedAt is DateTimeOffset startedAt)
                {
                    entries.Add(new MaintenanceHistoryEntryResponse(
                        $"defect:{defect.Id}:in_repair:{workOrder.Id}",
                        defect.AssetId,
                        "defect",
                        "defect_in_repair",
                        $"Defect in repair: {defect.Title}",
                        workOrder.WorkOrderNumber,
                        startedAt,
                        workOrder.CreatedByUserId,
                        "work_order",
                        workOrder.Id.ToString(),
                        defect.InspectionRunId?.ToString()));
                }
            }

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

            if (string.Equals(defect.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"defect:{defect.Id}:closed",
                    defect.AssetId,
                    "defect",
                    "defect_closed",
                    $"Defect closed: {defect.Title}",
                    defect.Status,
                    defect.UpdatedAt,
                    defect.ReportedByUserId,
                    "defect",
                    defect.Id.ToString(),
                    defect.InspectionRunId?.ToString()));
            }
        }

        var components = await db.AssetComponents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var component in components)
        {
            entries.Add(new MaintenanceHistoryEntryResponse(
                $"component:{component.Id}:created",
                assetId,
                "component",
                "component_created",
                $"Component recorded: {FormatComponentKey(component.ComponentKey)}",
                FormatComponentValue(component.ValueJson),
                component.CreatedAt,
                null,
                "asset_component",
                component.Id.ToString(),
                null));
        }

        var installedComponents = await db.AssetInstalledComponents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentAssetId == assetId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var component in installedComponents)
        {
            entries.Add(new MaintenanceHistoryEntryResponse(
                $"asset_component:{component.Id}:created",
                assetId,
                "component",
                "component_created",
                $"Component created: {component.ComponentNumber} · {component.Name}",
                $"{component.ComponentType} · {component.Status} · {component.Condition}",
                component.CreatedAt,
                null,
                "asset_installed_component",
                component.Id.ToString(),
                component.ParentComponentId?.ToString()));

            ComponentLifecycleEvent? lifecycleEvent = ResolveComponentLifecycleEvent(component);
            if (lifecycleEvent is not null)
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"asset_component:{component.Id}:{lifecycleEvent.EventKey}",
                    assetId,
                    "component",
                    lifecycleEvent.EventType,
                    lifecycleEvent.Title,
                    $"{component.ComponentNumber} · {component.ComponentType} · {component.Status}",
                    lifecycleEvent.OccurredAt,
                    null,
                    "asset_installed_component",
                    component.Id.ToString(),
                    component.ParentComponentId?.ToString()));
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

            var closeout = await db.WorkOrderCloseouts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id, cancellationToken);
            if (closeout is not null && string.Equals(closeout.FinalStatus, "closed", StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(new MaintenanceHistoryEntryResponse(
                    $"work_order:{workOrder.Id}:closed",
                    workOrder.AssetId,
                    "work_order",
                    "work_order_closed",
                    $"Work order closed: {workOrder.Title}",
                    closeout.FinalStatus,
                    closeout.ReturnToServiceAt ?? closeout.CreatedAt,
                    workOrder.CreatedByUserId,
                    "work_order_closeout",
                    closeout.Id.ToString(),
                    workOrder.DefectId?.ToString() ?? workOrder.PmScheduleId?.ToString()));
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

    private static string FormatComponentKey(string componentKey) =>
        componentKey
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Replace("Make", "Make")
            .Replace("Model", "Model")
            .Trim()
            .Replace("\t", " ");

    private static string FormatComponentValue(string valueJson)
    {
        try
        {
            using var document = JsonDocument.Parse(valueJson);
            return FormatJsonElement(document.RootElement);
        }
        catch
        {
            return string.IsNullOrWhiteSpace(valueJson) ? "Not recorded" : valueJson;
        }
    }

    private static string FormatJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "Not recorded",
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "Yes",
            JsonValueKind.False => "No",
            JsonValueKind.Null or JsonValueKind.Undefined => "Not recorded",
            JsonValueKind.Array => string.Join(", ", element.EnumerateArray().Select(FormatJsonElement).Where(x => !string.IsNullOrWhiteSpace(x))),
            JsonValueKind.Object => string.Join(
                "; ",
                element.EnumerateObject().Select(prop => $"{FormatComponentKey(prop.Name)}: {FormatJsonElement(prop.Value)}")),
            _ => element.ToString(),
        };
    }

    private static ComponentLifecycleEvent? ResolveComponentLifecycleEvent(AssetInstalledComponent component)
    {
        var occurredAt = component.Status.ToLowerInvariant() switch
        {
            "removed" => component.RemovedDate ?? component.UpdatedAt,
            "failed" => component.UpdatedAt,
            "replaced" => component.RemovedDate ?? component.UpdatedAt,
            "retired" => component.RemovedDate ?? component.UpdatedAt,
            _ => component.InstallDate ?? component.CreatedAt,
        };

        return component.Status.ToLowerInvariant() switch
        {
            "installed" => new ComponentLifecycleEvent("component_installed", "maintainarr.component.installed", $"Component installed: {component.ComponentNumber}", occurredAt),
            "removed" => new ComponentLifecycleEvent("component_removed", "maintainarr.component.removed", $"Component removed: {component.ComponentNumber}", occurredAt),
            "failed" => new ComponentLifecycleEvent("component_failed", "maintainarr.component.failed", $"Component failed: {component.ComponentNumber}", occurredAt),
            "replaced" => new ComponentLifecycleEvent("component_replaced", "maintainarr.component.replaced", $"Component replaced: {component.ComponentNumber}", occurredAt),
            "retired" => new ComponentLifecycleEvent("component_retired", "maintainarr.component.retired", $"Component retired: {component.ComponentNumber}", occurredAt),
            _ => null,
        };
    }

    private sealed record ComponentLifecycleEvent(
        string EventKey,
        string EventType,
        string Title,
        DateTimeOffset OccurredAt);
}
