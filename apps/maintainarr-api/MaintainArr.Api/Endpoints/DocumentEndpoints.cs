using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapMaintainArrDocumentEndpoints(this WebApplication app)
    {
        var docs = app.MapGroup("/api/v1/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        docs.MapGet("/", async (
            Guid? workOrderId,
            Guid? defectId,
            Guid? inspectionRunId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            DefectService defectService,
            InspectionRunService inspectionRunService,
            WorkOrderLaborEvidenceService workOrderEvidenceService,
            DefectEvidenceService defectEvidenceService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            if (workOrderId.HasValue)
            {
                authorization.RequireWorkOrdersRead(context.User);
                var detail = await workOrderService.GetAsync(tenantId, workOrderId.Value, cancellationToken);
                authorization.RequireWorkOrderAccess(context.User, detail.CreatedByUserId, detail.AssignedTechnicianPersonId);
                var items = await workOrderEvidenceService.ListEvidenceAsync(tenantId, workOrderId.Value, cancellationToken);
                return Results.Ok(items.Select(MapWorkOrder).ToList());
            }

            if (defectId.HasValue)
            {
                authorization.RequireDefectsRead(context.User);
                var detail = await defectService.GetAsync(tenantId, defectId.Value, cancellationToken);
                authorization.RequireDefectAccess(context.User, detail.ReportedByUserId);
                var items = await defectEvidenceService.ListDefectEvidenceAsync(tenantId, defectId.Value, cancellationToken);
                return Results.Ok(items.Select(MapDefect).ToList());
            }

            if (inspectionRunId.HasValue)
            {
                authorization.RequireInspectionsRead(context.User);
                var detail = await inspectionRunService.GetAsync(tenantId, inspectionRunId.Value, cancellationToken);
                authorization.RequireInspectionRunAccess(context.User, detail.StartedByUserId);
                var items = await defectEvidenceService.ListInspectionRunEvidenceAsync(tenantId, inspectionRunId.Value, cancellationToken);
                return Results.Ok(items.Select(MapInspection).ToList());
            }

            return Results.BadRequest(new
            {
                code = "documents.target_required",
                message = "Provide one target query: workOrderId, defectId, or inspectionRunId."
            });
        })
        .WithName("ListMaintainArrDocumentsV1");

        docs.MapGet("/alerts", async (
            string? targetType,
            Guid? assetId,
            int? limit,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DocumentAlertService alertService,
            CancellationToken cancellationToken) =>
        {
            var resolvedTargetType = string.IsNullOrWhiteSpace(targetType)
                ? "all"
                : targetType.Trim().ToLowerInvariant();

            if (resolvedTargetType is "all" or "defect")
            {
                authorization.RequireDefectsRead(context.User);
            }

            if (resolvedTargetType is "all" or "inspection_run")
            {
                authorization.RequireInspectionsRead(context.User);
            }

            if (resolvedTargetType is "all" or "work_order")
            {
                authorization.RequireWorkOrdersRead(context.User);
            }

            var tenantId = context.User.GetTenantId();
            var alerts = await alertService.ListMissingAlertsAsync(
                tenantId,
                resolvedTargetType,
                assetId,
                limit ?? 100,
                cancellationToken);
            return Results.Ok(alerts);
        })
        .WithName("ListMaintainArrDocumentAlertsV1");

        docs.MapPost("/", async (
            CreateMaintainArrDocumentRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            DefectService defectService,
            InspectionRunService inspectionRunService,
            WorkOrderLaborEvidenceService workOrderEvidenceService,
            DefectEvidenceService defectEvidenceService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var targetType = request.TargetType.Trim().ToLowerInvariant();
            var evidenceRequest = new CreateMaintainArrEvidenceRequest(
                request.EvidenceTypeKey,
                request.FileName,
                request.ContentType,
                request.ContentBase64,
                request.Notes);

            if (targetType == "work_order")
            {
                authorization.RequireWorkOrdersPerform(context.User);
                var detail = await workOrderService.GetAsync(tenantId, request.TargetId, cancellationToken);
                authorization.RequireWorkOrderAccess(context.User, detail.CreatedByUserId, detail.AssignedTechnicianPersonId);
                var created = await workOrderEvidenceService.UploadEvidenceAsync(
                    tenantId,
                    actorUserId,
                    request.TargetId,
                    new CreateWorkOrderEvidenceRequest(
                        request.EvidenceTypeKey,
                        request.FileName,
                        request.ContentType,
                        request.ContentBase64,
                        request.Notes),
                    cancellationToken);
                return Results.Created($"/api/v1/documents/{created.EvidenceId}", MapWorkOrder(created));
            }

            if (targetType == "defect")
            {
                authorization.RequireDefectsCreate(context.User);
                var detail = await defectService.GetAsync(tenantId, request.TargetId, cancellationToken);
                authorization.RequireDefectAccess(context.User, detail.ReportedByUserId);
                var created = await defectEvidenceService.UploadDefectEvidenceAsync(
                    tenantId,
                    actorUserId,
                    request.TargetId,
                    evidenceRequest,
                    cancellationToken);
                return Results.Created($"/api/v1/documents/{created.EvidenceId}", MapDefect(created));
            }

            if (targetType == "inspection_run")
            {
                authorization.RequireInspectionsExecute(context.User);
                var detail = await inspectionRunService.GetAsync(tenantId, request.TargetId, cancellationToken);
                authorization.RequireInspectionRunAccess(context.User, detail.StartedByUserId);
                var created = await defectEvidenceService.UploadInspectionRunEvidenceAsync(
                    tenantId,
                    actorUserId,
                    request.TargetId,
                    evidenceRequest,
                    cancellationToken);
                return Results.Created($"/api/v1/documents/{created.EvidenceId}", MapInspection(created));
            }

            return Results.BadRequest(new
            {
                code = "documents.target_invalid",
                message = "TargetType must be one of: work_order, defect, inspection_run."
            });
        })
        .WithName("CreateMaintainArrDocumentV1");
    }

    private static MaintainArrDocumentResponse MapWorkOrder(WorkOrderEvidenceResponse item) =>
        new(
            item.EvidenceId,
            "work_order",
            item.WorkOrderId,
            item.EvidenceTypeKey,
            item.FileName,
            item.ContentType,
            item.SizeBytes,
            item.Notes,
            item.UploadedByUserId,
            item.CreatedAt);

    private static MaintainArrDocumentResponse MapDefect(DefectEvidenceResponse item) =>
        new(
            item.EvidenceId,
            "defect",
            item.DefectId,
            item.EvidenceTypeKey,
            item.FileName,
            item.ContentType,
            item.SizeBytes,
            item.Notes,
            item.UploadedByUserId,
            item.CreatedAt);

    private static MaintainArrDocumentResponse MapInspection(InspectionRunEvidenceResponse item) =>
        new(
            item.EvidenceId,
            "inspection_run",
            item.InspectionRunId,
            item.EvidenceTypeKey,
            item.FileName,
            item.ContentType,
            item.SizeBytes,
            item.Notes,
            item.UploadedByUserId,
            item.CreatedAt);
}
