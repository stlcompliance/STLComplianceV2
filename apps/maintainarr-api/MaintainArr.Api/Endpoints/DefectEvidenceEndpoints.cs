using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DefectEvidenceEndpoints
{
    public static void MapMaintainArrDefectEvidenceEndpoints(this WebApplication app)
    {
        var defectEvidence = app.MapGroup("/api/defects/{defectId:guid}/evidence")
            .WithTags("DefectEvidence")
            .RequireAuthorization();

        defectEvidence.MapGet("/", async (
            Guid defectId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService defectService,
            DefectEvidenceService evidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await defectService.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByUserId);
            return Results.Ok(await evidenceService.ListDefectEvidenceAsync(tenantId, defectId, cancellationToken));
        })
        .WithName("ListDefectEvidence");

        defectEvidence.MapPost("/", async (
            Guid defectId,
            CreateMaintainArrEvidenceRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService defectService,
            DefectEvidenceService evidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await defectService.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByUserId);
            var created = await evidenceService.UploadDefectEvidenceAsync(
                tenantId,
                actorUserId,
                defectId,
                request,
                cancellationToken);
            return Results.Created($"/api/defects/{defectId}/evidence/{created.EvidenceId}", created);
        })
        .WithName("UploadDefectEvidence");

        var inspectionEvidence = app.MapGroup("/api/inspections/{inspectionRunId:guid}/evidence")
            .WithTags("InspectionRunEvidence")
            .RequireAuthorization();

        inspectionEvidence.MapGet("/", async (
            Guid inspectionRunId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService inspectionService,
            DefectEvidenceService evidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await inspectionService.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, detail.StartedByUserId);
            return Results.Ok(
                await evidenceService.ListInspectionRunEvidenceAsync(tenantId, inspectionRunId, cancellationToken));
        })
        .WithName("ListInspectionRunEvidence");

        inspectionEvidence.MapPost("/", async (
            Guid inspectionRunId,
            CreateMaintainArrEvidenceRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService inspectionService,
            DefectEvidenceService evidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsExecute(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await inspectionService.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, detail.StartedByUserId);
            var created = await evidenceService.UploadInspectionRunEvidenceAsync(
                tenantId,
                actorUserId,
                inspectionRunId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/inspections/{inspectionRunId}/evidence/{created.EvidenceId}",
                created);
        })
        .WithName("UploadInspectionRunEvidence");
    }
}
