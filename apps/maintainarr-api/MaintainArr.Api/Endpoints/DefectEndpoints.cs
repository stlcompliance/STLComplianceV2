using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DefectEndpoints
{
    public static void MapMaintainArrDefectEndpoints(this WebApplication app)
    {
        MapDefectRoutes(app.MapGroup("/api/defects").WithTags("Defects").RequireAuthorization(), string.Empty);
        MapDefectRoutes(app.MapGroup("/api/v1/defects").WithTags("Defects").RequireAuthorization(), "V1");

        MapInspectionRoutes(app.MapGroup("/api/inspections").WithTags("Inspections").RequireAuthorization(), string.Empty);
        MapInspectionRoutes(app.MapGroup("/api/v1/inspections").WithTags("Inspections").RequireAuthorization(), "V1");
    }

    private static void MapDefectRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            Guid? assetId,
            Guid? inspectionRunId,
            string? status,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllDefects(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                assetId,
                inspectionRunId,
                status,
                cancellationToken));
        })
        .WithName($"ListDefects{nameSuffix}");

        group.MapGet("/{defectId:guid}", async (
            Guid defectId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            return Results.Ok(detail);
        })
        .WithName($"GetDefect{nameSuffix}");

        group.MapPost("/", async (
            CreateDefectRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var created = await service.CreateManualAsync(tenantId, actorUserId, request, cancellationToken, actorPersonId);
            return Results.Created($"/api/defects/{created.DefectId}", created);
        })
        .WithName($"CreateDefect{nameSuffix}");

        group.MapPost("/drafts", async (
            UpsertDefectDraftRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var created = await service.CreateDraftAsync(tenantId, actorUserId, request, cancellationToken, actorPersonId);
            return Results.Created($"/api/defects/{created.DefectId}", created);
        })
        .WithName($"CreateDefectDraft{nameSuffix}");

        group.MapPatch("/{defectId:guid}/draft", async (
            Guid defectId,
            UpsertDefectDraftRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var updated = await service.UpdateDraftAsync(tenantId, actorUserId, defectId, request, cancellationToken, actorPersonId);
            return Results.Ok(updated);
        })
        .WithName($"UpdateDefectDraft{nameSuffix}");

        group.MapPost("/{defectId:guid}/validate", async (
            Guid defectId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            return Results.Ok(await service.ValidateDraftAsync(tenantId, defectId, cancellationToken));
        })
        .WithName($"ValidateDefectDraft{nameSuffix}");

        group.MapPost("/{defectId:guid}/duplicates", async (
            Guid defectId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            return Results.Ok(await service.CheckDuplicateDraftAsync(tenantId, defectId, cancellationToken));
        })
        .WithName($"CheckDefectDraftDuplicates{nameSuffix}");

        group.MapPost("/{defectId:guid}/preview", async (
            Guid defectId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.PreviewDraftAsync(tenantId, defectId, actorPersonId, cancellationToken));
        })
        .WithName($"PreviewDefectDraft{nameSuffix}");

        group.MapPost("/{defectId:guid}/submit", async (
            Guid defectId,
            SubmitDefectRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsSubmit(context.User);
            if (request.CreateWorkOrder)
            {
                authorization.RequireDefectWorkOrderHandoff(context.User);
            }

            if (request.MarkAssetNotReady)
            {
                authorization.RequireDefectReadinessManage(context.User);
            }

            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, detail.ReportedByPersonId, detail.ReportedByUserId);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var submitted = await service.SubmitAsync(tenantId, actorUserId, defectId, request, cancellationToken, actorPersonId);
            return Results.Ok(submitted);
        })
        .WithName($"SubmitDefect{nameSuffix}");

        group.MapPatch("/{defectId:guid}/status", async (
            Guid defectId,
            UpdateDefectStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsStatusManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, defectId, request, cancellationToken, actorPersonId);
            return Results.Ok(updated);
        })
        .WithName($"UpdateDefectStatus{nameSuffix}");
    }

    private static void MapInspectionRoutes(RouteGroupBuilder inspectionGroup, string nameSuffix)
    {
        inspectionGroup.MapPost("/{inspectionRunId:guid}/defects", async (
            Guid inspectionRunId,
            CreateDefectsFromInspectionRunRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService inspectionService,
            DefectService defectService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await inspectionService.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, existing.StartedByUserId);
            var result = await defectService.CreateFromInspectionRunAsync(
                tenantId,
                actorUserId,
                inspectionRunId,
                request,
                DefectSources.InspectionManual,
                cancellationToken,
                actorPersonId);
            return Results.Ok(result);
        })
        .WithName($"CreateDefectsFromInspectionRun{nameSuffix}");
    }
}
