using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PartCatalogEndpoints
{
    public static void MapSupplyArrPartCatalogEndpoints(this WebApplication app)
    {
        MapCatalogs(app.MapGroup("/api/catalogs").WithTags("PartCatalog").RequireAuthorization(), string.Empty);
        MapCatalogs(app.MapGroup("/api/v1/catalogs").WithTags("PartCatalog").RequireAuthorization(), "V1");

        MapParts(app.MapGroup("/api/parts").WithTags("PartCatalog").RequireAuthorization(), string.Empty);
        MapParts(app.MapGroup("/api/v1/parts").WithTags("PartCatalog").RequireAuthorization(), "V1");
        MapParts(app.MapGroup("/api/items").WithTags("PartCatalog").RequireAuthorization(), "Items");
        MapParts(app.MapGroup("/api/v1/items").WithTags("PartCatalog").RequireAuthorization(), "ItemsV1");
    }

    private static void MapCatalogs(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartCatalogService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListPartCatalogs{nameSuffix}");

        group.MapGet("/{catalogId:guid}", async (
            Guid catalogId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartCatalogService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, catalogId, cancellationToken));
        })
        .WithName($"GetPartCatalog{nameSuffix}");

        group.MapPost("/", async (
            CreatePartCatalogRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartCatalogService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/catalogs/{created.CatalogId}", created);
        })
        .WithName($"CreatePartCatalog{nameSuffix}");

        group.MapPut("/{catalogId:guid}", async (
            Guid catalogId,
            UpdatePartCatalogRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartCatalogService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(tenantId, actorUserId, catalogId, request, cancellationToken));
        })
        .WithName($"UpdatePartCatalog{nameSuffix}");

        group.MapPatch("/{catalogId:guid}/status", async (
            Guid catalogId,
            UpdatePartCatalogStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartCatalogService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                catalogId,
                request,
                cancellationToken));
        })
        .WithName($"UpdatePartCatalogStatus{nameSuffix}");
    }

    private static void MapParts(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            Guid? catalogId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, catalogId, cancellationToken));
        })
        .WithName($"ListParts{nameSuffix}");

        group.MapGet("/{partId:guid}", async (
            Guid partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, partId, cancellationToken));
        })
        .WithName($"GetPart{nameSuffix}");

        group.MapPost("/", async (
            CreatePartRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/parts/{created.PartId}", created);
        })
        .WithName($"CreatePart{nameSuffix}");

        group.MapPut("/{partId:guid}", async (
            Guid partId,
            UpdatePartRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(tenantId, actorUserId, partId, request, cancellationToken));
        })
        .WithName($"UpdatePart{nameSuffix}");

        group.MapPatch("/{partId:guid}/status", async (
            Guid partId,
            UpdatePartStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                partId,
                request,
                cancellationToken));
        })
        .WithName($"UpdatePartStatus{nameSuffix}");

        group.MapPost("/{partId:guid}/manufacturer-aliases", async (
            Guid partId,
            CreatePartManufacturerAliasRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var alias = await service.AddManufacturerAliasAsync(
                tenantId,
                actorUserId,
                partId,
                request,
                cancellationToken);
            return Results.Created($"/api/parts/{partId}/manufacturer-aliases/{alias.AliasId}", alias);
        })
        .WithName($"CreatePartManufacturerAlias{nameSuffix}");

        group.MapPost("/{partId:guid}/sources", async (
            Guid partId,
            CreatePartSourceRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var source = await service.AddSourceAsync(
                tenantId,
                actorUserId,
                partId,
                request,
                cancellationToken);
            return Results.Created($"/api/parts/{partId}/sources/{source.SourceId}", source);
        })
        .WithName($"CreatePartSource{nameSuffix}");

        MapPartSupplierLinkRoutes(group, nameSuffix);
    }

    private static void MapPartSupplierLinkRoutes(
        RouteGroupBuilder group,
        string nameSuffix)
    {
        group.MapPost("/{partId:guid}/supplier-links", async (
            Guid partId,
            CreatePartSupplierLinkRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var link = await service.AddSupplierLinkAsync(
                tenantId,
                actorUserId,
                partId,
                request,
                cancellationToken);
            return Results.Created($"/api/parts/{partId}/supplier-links/{link.LinkId}", link);
        })
        .WithName($"CreatePartSupplierLink{nameSuffix}");

        group.MapPut("/{partId:guid}/supplier-links/{linkId:guid}/catalog-price", async (
            Guid partId,
            Guid linkId,
            UpsertPartSupplierLinkCatalogPriceRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertSupplierLinkCatalogPriceAsync(
                tenantId,
                actorUserId,
                partId,
                linkId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertPartSupplierLinkCatalogPrice{nameSuffix}");

        group.MapPut("/{partId:guid}/supplier-links/{linkId:guid}/catalog-lead-time", async (
            Guid partId,
            Guid linkId,
            UpsertPartSupplierLinkCatalogLeadTimeRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertSupplierLinkCatalogLeadTimeAsync(
                tenantId,
                actorUserId,
                partId,
                linkId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertPartSupplierLinkCatalogLeadTime{nameSuffix}");

        group.MapPut("/{partId:guid}/supplier-links/{linkId:guid}/catalog-availability", async (
            Guid partId,
            Guid linkId,
            UpsertPartSupplierLinkCatalogAvailabilityRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertSupplierLinkCatalogAvailabilityAsync(
                tenantId,
                actorUserId,
                partId,
                linkId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertPartSupplierLinkCatalogAvailability{nameSuffix}");
    }
}
