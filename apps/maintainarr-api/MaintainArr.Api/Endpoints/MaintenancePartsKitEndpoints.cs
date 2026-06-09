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
            authorization.RequirePartsKitsRead(context.User);
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
            authorization.RequirePartsKitsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, partsKitId, cancellationToken));
        })
        .WithName($"GetMaintenancePartsKit{nameSuffix}");

        group.MapPost("/validate", async (
            MaintenancePartsKitPreviewRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsValidate(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ValidateAsync(tenantId, request, cancellationToken));
        })
        .WithName($"ValidateMaintenancePartsKit{nameSuffix}");

        group.MapPost("/preview", async (
            MaintenancePartsKitPreviewRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsPreview(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PreviewAsync(tenantId, request, cancellationToken));
        })
        .WithName($"PreviewMaintenancePartsKit{nameSuffix}");

        group.MapPost("/", async (
            CreateMaintenancePartsKitRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var created = await service.CreateAsync(tenantId, actorUserId, actorPersonId, request, cancellationToken);
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
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateAsync(tenantId, actorUserId, actorPersonId, partsKitId, request, cancellationToken);
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
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, actorPersonId, partsKitId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateMaintenancePartsKitStatus{nameSuffix}");

        group.MapPost("/{partsKitId:guid}/submit-approval", async (
            Guid partsKitId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.SubmitForApprovalAsync(tenantId, actorUserId, actorPersonId, partsKitId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"SubmitMaintenancePartsKitForApproval{nameSuffix}");

        group.MapPost("/{partsKitId:guid}/activate", async (
            Guid partsKitId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsActivate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.ActivateAsync(tenantId, actorUserId, actorPersonId, partsKitId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"ActivateMaintenancePartsKit{nameSuffix}");

        group.MapPost("/{partsKitId:guid}/retire", async (
            Guid partsKitId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsRetire(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.RetireAsync(tenantId, actorUserId, actorPersonId, partsKitId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"RetireMaintenancePartsKit{nameSuffix}");

        group.MapPost("/{partsKitId:guid}/clone", async (
            Guid partsKitId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsClone(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var created = await service.CloneAsync(tenantId, actorUserId, actorPersonId, partsKitId, cancellationToken);
            return Results.Created($"/api/v1/maintenance-parts-kits/{created.PartsKitId}", created);
        })
        .WithName($"CloneMaintenancePartsKit{nameSuffix}");

        group.MapPost("/{partsKitId:guid}/lines", async (
            Guid partsKitId,
            CreateMaintenancePartsKitLineRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenancePartsKitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var created = await service.AddLineAsync(tenantId, actorUserId, actorPersonId, partsKitId, request, cancellationToken);
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
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateLineAsync(tenantId, actorUserId, actorPersonId, partsKitId, lineId, request, cancellationToken);
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
            authorization.RequirePartsKitsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            await service.DeleteLineAsync(tenantId, actorUserId, actorPersonId, partsKitId, lineId, cancellationToken);
            return Results.NoContent();
        })
        .WithName($"DeleteMaintenancePartsKitLine{nameSuffix}");
    }
}
