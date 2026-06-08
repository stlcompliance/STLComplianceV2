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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var created = await service.CreateAsync(tenantId, actorUserId, actorPersonId, request, cancellationToken);
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateAsync(tenantId, actorUserId, actorPersonId, inspectionTemplateId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateInspectionTemplate{nameSuffix}");

        group.MapGet("/{inspectionTemplateId:guid}/validate", async (
            Guid inspectionTemplateId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ValidateAsync(tenantId, inspectionTemplateId, cancellationToken));
        })
        .WithName($"ValidateInspectionTemplate{nameSuffix}");

        group.MapGet("/{inspectionTemplateId:guid}/preview", async (
            Guid inspectionTemplateId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PreviewAsync(tenantId, inspectionTemplateId, cancellationToken));
        })
        .WithName($"PreviewInspectionTemplate{nameSuffix}");

        group.MapPost("/{inspectionTemplateId:guid}/publish", async (
            Guid inspectionTemplateId,
            PublishInspectionTemplateRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var published = await service.PublishAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Ok(published);
        })
        .WithName($"PublishInspectionTemplate{nameSuffix}");

        group.MapPost("/{inspectionTemplateId:guid}/retire", async (
            Guid inspectionTemplateId,
            RetireInspectionTemplateRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var retired = await service.RetireAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Ok(retired);
        })
        .WithName($"RetireInspectionTemplate{nameSuffix}");

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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                inspectionTemplateId,
                request,
                cancellationToken: cancellationToken);
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var cloned = await service.CloneAsync(
                tenantId,
                actorUserId,
                actorPersonId,
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var created = await service.CreateCategoryAsync(
                tenantId,
                actorUserId,
                actorPersonId,
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateCategoryAsync(
                tenantId,
                actorUserId,
                actorPersonId,
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            await service.DeleteCategoryAsync(tenantId, actorUserId, actorPersonId, inspectionTemplateId, categoryId, cancellationToken);
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var created = await service.CreateChecklistItemAsync(
                tenantId,
                actorUserId,
                actorPersonId,
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.UpdateChecklistItemAsync(
                tenantId,
                actorUserId,
                actorPersonId,
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            await service.DeleteChecklistItemAsync(
                tenantId,
                actorUserId,
                actorPersonId,
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
            var actorPersonId = context.User.GetPersonId().ToString("D");
            var updated = await service.ReplaceAssetTypesAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                inspectionTemplateId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"ReplaceInspectionTemplateAssetTypes{nameSuffix}");
    }
}
