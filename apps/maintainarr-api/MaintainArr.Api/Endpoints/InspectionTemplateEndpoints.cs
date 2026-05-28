using MaintainArr.Api.Contracts;

using MaintainArr.Api.Services;

using STLCompliance.Shared.Auth;



namespace MaintainArr.Api.Endpoints;



public static class InspectionTemplateEndpoints

{

    public static void MapMaintainArrInspectionTemplateEndpoints(this WebApplication app)

    {

        var group = app.MapGroup("/api/inspection-templates").WithTags("InspectionTemplates").RequireAuthorization();



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

        .WithName("ListInspectionTemplates");



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

        .WithName("GetInspectionTemplate");



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

        .WithName("CreateInspectionTemplate");



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

        .WithName("UpdateInspectionTemplate");



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

        .WithName("UpdateInspectionTemplateStatus");



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

        .WithName("CreateInspectionTemplateCategory");



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

        .WithName("UpdateInspectionTemplateCategory");



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

        .WithName("DeleteInspectionTemplateCategory");



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

        .WithName("CreateInspectionChecklistItem");



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

        .WithName("UpdateInspectionChecklistItem");



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

        .WithName("DeleteInspectionChecklistItem");



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

        .WithName("ReplaceInspectionTemplateAssetTypes");

    }

}


