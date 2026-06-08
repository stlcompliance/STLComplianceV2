using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DefectEvidenceEndpoints
{
    public static void MapMaintainArrDefectEvidenceEndpoints(this WebApplication app)
    {
        MapDefectEvidenceRoutes(app.MapGroup("/api/defects/{defectId:guid}/evidence").WithTags("DefectEvidence").RequireAuthorization(), string.Empty);
        MapDefectEvidenceRoutes(app.MapGroup("/api/v1/defects/{defectId:guid}/evidence").WithTags("DefectEvidence").RequireAuthorization(), "V1");

        MapInspectionEvidenceRoutes(app.MapGroup("/api/inspections/{inspectionRunId:guid}/evidence").WithTags("InspectionRunEvidence").RequireAuthorization(), string.Empty);
        MapInspectionEvidenceRoutes(app.MapGroup("/api/v1/inspections/{inspectionRunId:guid}/evidence").WithTags("InspectionRunEvidence").RequireAuthorization(), "V1");
    }

    private static void MapDefectEvidenceRoutes(RouteGroupBuilder defectEvidence, string nameSuffix)
    {
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
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            return Results.Ok(await evidenceService.ListDefectEvidenceAsync(tenantId, defectId, cancellationToken));
        })
        .WithName($"ListDefectEvidence{nameSuffix}");

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
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            var created = await evidenceService.UploadDefectEvidenceAsync(
                tenantId,
                actorUserId,
                defectId,
                request,
                cancellationToken);
            return Results.Created($"/api/defects/{defectId}/evidence/{created.EvidenceId}", created);
        })
        .WithName($"UploadDefectEvidence{nameSuffix}");
    }

    private static void MapInspectionEvidenceRoutes(RouteGroupBuilder inspectionEvidence, string nameSuffix)
    {
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
        .WithName($"ListInspectionRunEvidence{nameSuffix}");

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
        .WithName($"UploadInspectionRunEvidence{nameSuffix}");
    }
}
