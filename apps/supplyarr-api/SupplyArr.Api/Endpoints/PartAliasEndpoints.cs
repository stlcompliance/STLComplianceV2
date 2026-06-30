using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PartAliasEndpoints
{
    public static void MapSupplyArrPartAliasEndpoints(this WebApplication app)
    {
        MapItemCategoryRoutes(app.MapGroup("/api/v1/item-categories").WithTags("PartCatalog").RequireAuthorization());
        MapManufacturerRoutes(app.MapGroup("/api/v1/manufacturers").WithTags("PartCatalog").RequireAuthorization());
        MapVendorItemRoutes(app.MapGroup("/api/v1/vendor-items").WithTags("PartCatalog").RequireAuthorization());
    }

    private static void MapItemCategoryRoutes(RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var items = await service.ListAsync(tenantId, null, cancellationToken);
            var categories = items
                .GroupBy(x => x.CategoryKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ItemCategorySummaryResponse(g.Key, g.Count()))
                .OrderBy(x => x.CategoryKey)
                .ToList();
            return Results.Ok(categories);
        })
        .WithName("ListItemCategoriesV1");
    }

    private static void MapManufacturerRoutes(RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var items = await service.ListAsync(tenantId, null, cancellationToken);
            var manufacturers = items
                .Where(x => !string.IsNullOrWhiteSpace(x.ManufacturerName))
                .GroupBy(x => x.ManufacturerName.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new ManufacturerSummaryResponse(g.Key, g.Count()))
                .OrderBy(x => x.ManufacturerName)
                .ToList();
            return Results.Ok(manufacturers);
        })
        .WithName("ListManufacturersV1");
    }

    private static void MapVendorItemRoutes(RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            Guid? partyId,
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var items = await service.ListAsync(tenantId, null, cancellationToken);
            var vendorItems = items
                .Where(p => !partId.HasValue || p.PartId == partId.Value)
                .SelectMany(p => p.VendorLinks.Select(v => new VendorItemResponse(
                    v.LinkId,
                    p.PartId,
                    p.PartKey,
                    p.DisplayName,
                    p.CategoryKey,
                    v.PartyId,
                    v.PartyKey,
                    v.PartyDisplayName,
                    v.VendorPartNumber,
                    v.IsPreferred,
                    v.CatalogUnitPrice,
                    v.CatalogCurrencyCode,
                    v.CatalogMinimumOrderQuantity,
                    v.CatalogLeadTimeDays,
                    v.CatalogQuantityAvailable,
                    v.CatalogAvailabilityStatus,
                    v.CreatedAt)))
                .Where(x => !partyId.HasValue || x.PartyId == partyId.Value)
                .OrderBy(x => x.PartKey)
                .ToList();
            return Results.Ok(vendorItems);
        })
        .WithName("ListVendorItemsV1");

        group.MapPost("/", async (
            CreateVendorItemRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.AddVendorLinkAsync(
                tenantId,
                actorUserId,
                request.PartId,
                new CreatePartVendorLinkRequest(
                    request.PartyId,
                    request.PartyId,
                    request.PartyId,
                    request.VendorPartNumber,
                    request.IsPreferred),
                cancellationToken);
            return Results.Created($"/api/v1/vendor-items/{created.LinkId}", created);
        })
        .WithName("CreateVendorItemV1");
    }
}
