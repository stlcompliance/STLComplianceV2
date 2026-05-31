using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class DocumentAlertService(MaintainArrDbContext db)
{
    public async Task<IReadOnlyList<MaintainArrDocumentAlertResponse>> ListMissingAlertsAsync(
        Guid tenantId,
        string targetType,
        Guid? assetId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedTargetType = string.IsNullOrWhiteSpace(targetType)
            ? "all"
            : targetType.Trim().ToLowerInvariant();
        if (normalizedTargetType is not ("all" or "defect" or "inspection_run" or "work_order"))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "documents.target_invalid",
                "targetType must be one of: all, defect, inspection_run, work_order.",
                400);
        }

        var safeLimit = Math.Clamp(limit, 1, 500);
        var alerts = new List<MaintainArrDocumentAlertResponse>();

        if (normalizedTargetType is "all" or "defect")
        {
            var defectAlerts = await (
                from defect in db.Defects.AsNoTracking()
                join asset in db.Assets.AsNoTracking() on defect.AssetId equals asset.Id
                where defect.TenantId == tenantId
                    && asset.TenantId == tenantId
                    && (assetId == null || defect.AssetId == assetId.Value)
                    && (defect.Status == DefectStatuses.Open
                        || defect.Status == DefectStatuses.Acknowledged
                        || defect.Status == DefectStatuses.InRepair)
                    && !db.DefectEvidence.Any(e => e.TenantId == tenantId && e.DefectId == defect.Id)
                orderby defect.UpdatedAt descending
                select new MaintainArrDocumentAlertResponse(
                    "missing_document",
                    "defect",
                    defect.Id,
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    defect.Title,
                    $"Open defect \"{defect.Title}\" is missing evidence documents.",
                    defect.UpdatedAt))
                .Take(safeLimit)
                .ToListAsync(cancellationToken);

            alerts.AddRange(defectAlerts);
        }

        if (normalizedTargetType is "all" or "inspection_run")
        {
            var inspectionAlerts = await (
                from run in db.InspectionRuns.AsNoTracking()
                join asset in db.Assets.AsNoTracking() on run.AssetId equals asset.Id
                join template in db.InspectionTemplates.AsNoTracking() on run.InspectionTemplateId equals template.Id
                where run.TenantId == tenantId
                    && asset.TenantId == tenantId
                    && template.TenantId == tenantId
                    && (assetId == null || run.AssetId == assetId.Value)
                    && run.Status == InspectionRunStatuses.Completed
                    && run.Result == InspectionRunResults.Failed
                    && !db.InspectionRunEvidence.Any(e => e.TenantId == tenantId && e.InspectionRunId == run.Id)
                orderby run.CompletedAt descending
                select new MaintainArrDocumentAlertResponse(
                    "missing_document",
                    "inspection_run",
                    run.Id,
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    template.Name,
                    $"Failed inspection \"{template.Name}\" is missing evidence documents.",
                    run.CompletedAt ?? run.UpdatedAt))
                .Take(safeLimit)
                .ToListAsync(cancellationToken);

            alerts.AddRange(inspectionAlerts);
        }

        if (normalizedTargetType is "all" or "work_order")
        {
            var workOrderAlerts = await (
                from workOrder in db.WorkOrders.AsNoTracking()
                join asset in db.Assets.AsNoTracking() on workOrder.AssetId equals asset.Id
                where workOrder.TenantId == tenantId
                    && asset.TenantId == tenantId
                    && (assetId == null || workOrder.AssetId == assetId.Value)
                    && workOrder.Status == WorkOrderStatuses.Completed
                    && !db.WorkOrderEvidence.Any(e => e.TenantId == tenantId && e.WorkOrderId == workOrder.Id)
                orderby workOrder.CompletedAt descending
                select new MaintainArrDocumentAlertResponse(
                    "missing_document",
                    "work_order",
                    workOrder.Id,
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    workOrder.Title,
                    $"Completed work order \"{workOrder.WorkOrderNumber}\" is missing evidence documents.",
                    workOrder.CompletedAt ?? workOrder.UpdatedAt))
                .Take(safeLimit)
                .ToListAsync(cancellationToken);

            alerts.AddRange(workOrderAlerts);
        }

        return alerts
            .OrderByDescending(x => x.DetectedAt)
            .Take(safeLimit)
            .ToList();
    }
}
