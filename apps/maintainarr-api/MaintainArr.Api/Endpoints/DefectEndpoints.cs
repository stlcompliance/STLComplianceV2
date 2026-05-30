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
            return Results.Ok(await service.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
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
            authorization.RequireDefectAccess(context.User, detail.ReportedByUserId);
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
            var created = await service.CreateManualAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/defects/{created.DefectId}", created);
        })
        .WithName($"CreateDefect{nameSuffix}");

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
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, defectId, request, cancellationToken);
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
            var existing = await inspectionService.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, existing.StartedByUserId);
            var result = await defectService.CreateFromInspectionRunAsync(
                tenantId,
                actorUserId,
                inspectionRunId,
                request,
                DefectSources.InspectionManual,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"CreateDefectsFromInspectionRun{nameSuffix}");
    }
}
