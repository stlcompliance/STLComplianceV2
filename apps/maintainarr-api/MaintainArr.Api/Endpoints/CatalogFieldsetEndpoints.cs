using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class CatalogFieldsetEndpoints
{
    public static void MapMaintainArrCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/catalogs").WithTags("Catalogs").RequireAuthorization();

        group.MapGet("/", async (string? keys, HttpContext context, MaintainArrAuthorizationService authorization, CatalogService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var parsedKeys = string.IsNullOrWhiteSpace(keys) ? null : keys.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return Results.Ok(await service.ListAsync(tenantId, parsedKeys, cancellationToken));
        });

        group.MapGet("/{catalogKey}", async (string catalogKey, HttpContext context, MaintainArrAuthorizationService authorization, CatalogService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, catalogKey, cancellationToken));
        });

        group.MapPost("/{catalogKey}/options", async (string catalogKey, UpsertCatalogOptionRequest request, HttpContext context, MaintainArrAuthorizationService authorization, CatalogService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var option = await service.UpsertOptionAsync(tenantId, catalogKey, request.Key, request, cancellationToken);
            return Results.Ok(option);
        });

        group.MapPatch("/{catalogKey}/options/{optionKey}", async (string catalogKey, string optionKey, UpsertCatalogOptionRequest request, HttpContext context, MaintainArrAuthorizationService authorization, CatalogService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var option = await service.UpsertOptionAsync(tenantId, catalogKey, optionKey, request, cancellationToken);
            return Results.Ok(option);
        });
    }

    public static void MapMaintainArrFieldsetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/fieldsets").WithTags("Fieldsets").RequireAuthorization();

        group.MapGet("/assets", async (HttpContext context, MaintainArrAuthorizationService authorization, FieldsetService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await service.GetAssetsFieldsetAsync(context.User.GetTenantId(), "default", cancellationToken));
        });

        group.MapGet("/assets/create", async (HttpContext context, MaintainArrAuthorizationService authorization, FieldsetService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await service.GetAssetsFieldsetAsync(context.User.GetTenantId(), "create", cancellationToken));
        });

        group.MapGet("/assets/{assetId:guid}/edit", async (Guid assetId, HttpContext context, MaintainArrAuthorizationService authorization, FieldsetService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await service.GetAssetsFieldsetAsync(context.User.GetTenantId(), "edit", cancellationToken));
        });

        group.MapGet("/work-orders/create", async (HttpContext context, MaintainArrAuthorizationService authorization, FieldsetService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            return Results.Ok(await service.GetWorkOrdersFieldsetAsync(context.User.GetTenantId(), "create", cancellationToken));
        });

        group.MapGet("/defects/create", async (HttpContext context, MaintainArrAuthorizationService authorization, FieldsetService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectsCreate(context.User);
            return Results.Ok(await service.GetDefectsFieldsetAsync(context.User.GetTenantId(), "create", cancellationToken));
        });

        group.MapGet("/inspection-templates/create", async (HttpContext context, MaintainArrAuthorizationService authorization, FieldsetService service, CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsManage(context.User);
            return Results.Ok(await service.GetInspectionTemplatesFieldsetAsync(context.User.GetTenantId(), "create", cancellationToken));
        });
    }
}
