using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenancePartsKitEndpoints
{
    public static void MapMaintainArrMaintenancePartsKitEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/maintenance-parts-kits").WithTags("MaintenancePartsKits").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/maintenance-parts-kits").WithTags("MaintenancePartsKits").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListMaintenancePartsKits{nameSuffix}");

        group.MapGet("/{partsKitId:guid}", async (
            Guid partsKitId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, partsKitId, cancellationToken));
        })
        .WithName($"GetMaintenancePartsKit{nameSuffix}");

        group.MapPost("/", async (
            CreateMaintenancePartsKitRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/v1/maintenance-parts-kits/{created.PartsKitId}", created);
        })
        .WithName($"CreateMaintenancePartsKit{nameSuffix}");

        group.MapPut("/{partsKitId:guid}", async (
            Guid partsKitId,
            UpdateMaintenancePartsKitRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, partsKitId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateMaintenancePartsKit{nameSuffix}");

        group.MapPatch("/{partsKitId:guid}/status", async (
            Guid partsKitId,
            UpdateMaintenancePartsKitStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, partsKitId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateMaintenancePartsKitStatus{nameSuffix}");

        group.MapPost("/{partsKitId:guid}/lines", async (
            Guid partsKitId,
            CreateMaintenancePartsKitLineRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.AddLineAsync(tenantId, actorUserId, partsKitId, request, cancellationToken);
            return Results.Created($"/api/v1/maintenance-parts-kits/{partsKitId}/lines/{created.PartsKitLineId}", created);
        })
        .WithName($"CreateMaintenancePartsKitLine{nameSuffix}");

        group.MapPut("/{partsKitId:guid}/lines/{lineId:guid}", async (
            Guid partsKitId,
            Guid lineId,
            UpdateMaintenancePartsKitLineRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateLineAsync(tenantId, actorUserId, partsKitId, lineId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateMaintenancePartsKitLine{nameSuffix}");

        group.MapDelete("/{partsKitId:guid}/lines/{lineId:guid}", async (
            Guid partsKitId,
            Guid lineId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteLineAsync(tenantId, actorUserId, partsKitId, lineId, cancellationToken);
            return Results.NoContent();
        })
        .WithName($"DeleteMaintenancePartsKitLine{nameSuffix}");
    }
}
