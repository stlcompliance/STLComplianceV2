using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class InspectionTemplateEndpoints
{
    public static void MapMaintainArrInspectionTemplateEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/inspection-templates").WithTags("InspectionTemplates").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/inspection-templates").WithTags("InspectionTemplates").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListInspectionTemplates{nameSuffix}");

        group.MapGet("/{inspectionTemplateId:guid}", async (
            Guid inspectionTemplateId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, inspectionTemplateId, cancellationToken));
        })
        .WithName($"GetInspectionTemplate{nameSuffix}");

        group.MapPost("/", async (
            CreateInspectionTemplateRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/inspection-templates/{created.InspectionTemplateId}", created);
        })
        .WithName($"CreateInspectionTemplate{nameSuffix}");

        group.MapPut("/{inspectionTemplateId:guid}", async (
            Guid inspectionTemplateId,
            UpdateInspectionTemplateRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, inspectionTemplateId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateInspectionTemplate{nameSuffix}");

        group.MapPatch("/{inspectionTemplateId:guid}/status", async (
            Guid inspectionTemplateId,
            UpdateInspectionTemplateStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateInspectionTemplateStatus{nameSuffix}");

        group.MapPost("/{inspectionTemplateId:guid}/clone", async (
            Guid inspectionTemplateId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var cloned = await service.CloneAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                cancellationToken);
            return Results.Created($"/api/inspection-templates/{cloned.InspectionTemplateId}", cloned);
        })
        .WithName($"CloneInspectionTemplate{nameSuffix}");

        group.MapPost("/{inspectionTemplateId:guid}/categories", async (
            Guid inspectionTemplateId,
            CreateInspectionTemplateCategoryRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateCategoryAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/inspection-templates/{inspectionTemplateId}/categories/{created.CategoryId}",
                created);
        })
        .WithName($"CreateInspectionTemplateCategory{nameSuffix}");

        group.MapPut("/{inspectionTemplateId:guid}/categories/{categoryId:guid}", async (
            Guid inspectionTemplateId,
            Guid categoryId,
            UpdateInspectionTemplateCategoryRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateCategoryAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                categoryId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateInspectionTemplateCategory{nameSuffix}");

        group.MapDelete("/{inspectionTemplateId:guid}/categories/{categoryId:guid}", async (
            Guid inspectionTemplateId,
            Guid categoryId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteCategoryAsync(tenantId, actorUserId, inspectionTemplateId, categoryId, cancellationToken);
            return Results.NoContent();
        })
        .WithName($"DeleteInspectionTemplateCategory{nameSuffix}");

        group.MapPost("/{inspectionTemplateId:guid}/checklist-items", async (
            Guid inspectionTemplateId,
            CreateInspectionChecklistItemRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateChecklistItemAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/inspection-templates/{inspectionTemplateId}/checklist-items/{created.ChecklistItemId}",
                created);
        })
        .WithName($"CreateInspectionChecklistItem{nameSuffix}");

        group.MapPut("/{inspectionTemplateId:guid}/checklist-items/{checklistItemId:guid}", async (
            Guid inspectionTemplateId,
            Guid checklistItemId,
            UpdateInspectionChecklistItemRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateChecklistItemAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                checklistItemId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateInspectionChecklistItem{nameSuffix}");

        group.MapDelete("/{inspectionTemplateId:guid}/checklist-items/{checklistItemId:guid}", async (
            Guid inspectionTemplateId,
            Guid checklistItemId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteChecklistItemAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                checklistItemId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName($"DeleteInspectionChecklistItem{nameSuffix}");

        group.MapPut("/{inspectionTemplateId:guid}/asset-types", async (
            Guid inspectionTemplateId,
            ReplaceInspectionTemplateAssetTypesRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.ReplaceAssetTypesAsync(
                tenantId,
                actorUserId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"ReplaceInspectionTemplateAssetTypes{nameSuffix}");
    }
}
