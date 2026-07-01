using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ExternalReferenceEndpoints
{
    private const int MaxBatchResolveItems = 100;

    private static readonly ExternalReferenceEntityContract[] EntityContracts =
    [
        new("supplier", "Supplier identity and supplier sub-unit records.", "/api/v1/references/resolve?entityType=supplier&key={supplierKey}"),
        new("part", "Part/material item master records.", "/api/v1/references/resolve?entityType=part&key={partKey}"),
        new("purchase_request", "Purchase request records.", "/api/v1/references/resolve?entityType=purchase_request&key={requestKey}"),
        new("purchase_order", "Purchase order records.", "/api/v1/references/resolve?entityType=purchase_order&key={orderKey}"),
        new("receipt", "Receiving receipt records.", "/api/v1/references/resolve?entityType=receipt&key={receiptKey}"),
        new("warranty_claim", "Warranty claim records.", "/api/v1/references/resolve?entityType=warranty_claim&key={claimKey}")
    ];

    public static void MapSupplyArrExternalReferenceEndpoints(this WebApplication app)
    {
        MapGroup(app, "/api/references");
        MapGroup(app, "/api/v1/references");
    }

    private static void MapGroup(WebApplication app, string prefix)
    {
        var group = app.MapGroup(prefix).WithTags("ExternalReferences").RequireAuthorization();
        group.MapGet("/", IndexAsync).WithName($"ListSupplyArrExternalReferenceContracts{RouteSuffix(prefix)}");
        group.MapGet("/resolve", ResolveAsync).WithName($"ResolveSupplyArrExternalReference{RouteSuffix(prefix)}");
        group.MapPost("/resolve-batch", ResolveBatchAsync).WithName($"ResolveBatchSupplyArrExternalReference{RouteSuffix(prefix)}");
    }

    private static IResult IndexAsync(
        SupplyArrAuthorizationService authorization,
        HttpContext context)
    {
        authorization.RequireSuppliersRead(context.User);
        return Results.Ok(new ExternalReferenceContractIndexResponse(
            EntityContracts,
            DateTimeOffset.UtcNow));
    }

    private static async Task<IResult> ResolveAsync(
        string entityType,
        string key,
        HttpContext context,
        SupplyArrAuthorizationService authorization,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(key))
        {
            return Results.BadRequest(new { error = "entityType and key are required." });
        }

        var tenantId = context.User.GetTenantId();
        var normalizedEntityType = entityType.Trim().ToLowerInvariant();
        var normalizedKey = key.Trim();

        var response = await ResolveReferenceAsync(
            normalizedEntityType,
            normalizedKey,
            tenantId,
            authorization,
            context,
            db,
            cancellationToken);

        return response is null
            ? Results.NotFound()
            : Results.Ok(response);
    }

    private static async Task<IResult> ResolveBatchAsync(
        ExternalReferenceBatchResolveRequest request,
        HttpContext context,
        SupplyArrAuthorizationService authorization,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return Results.BadRequest(new { error = "items are required." });
        }

        if (request.Items.Count > MaxBatchResolveItems)
        {
            return Results.BadRequest(new { error = $"at most {MaxBatchResolveItems} items are allowed." });
        }

        var tenantId = context.User.GetTenantId();
        var responses = new List<ExternalReferenceBatchResolveItemResponse>(request.Items.Count);
        for (var i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (string.IsNullOrWhiteSpace(item.EntityType) || string.IsNullOrWhiteSpace(item.Key))
            {
                responses.Add(new ExternalReferenceBatchResolveItemResponse(
                    i,
                    item.EntityType,
                    item.Key,
                    Found: false,
                    Resolution: null));
                continue;
            }

            var normalizedEntityType = item.EntityType.Trim().ToLowerInvariant();
            var normalizedKey = item.Key.Trim();
            var resolution = await ResolveReferenceAsync(
                normalizedEntityType,
                normalizedKey,
                tenantId,
                authorization,
                context,
                db,
                cancellationToken);
            responses.Add(new ExternalReferenceBatchResolveItemResponse(
                i,
                item.EntityType,
                item.Key,
                Found: resolution is not null,
                Resolution: resolution));
        }

        return Results.Ok(new ExternalReferenceBatchResolveResponse(
            responses,
            DateTimeOffset.UtcNow));
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolveReferenceAsync(
        string normalizedEntityType,
        string normalizedKey,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        return normalizedEntityType switch
        {
            "supplier" => await ResolveSupplierAsync(
                normalizedKey,
                tenantId,
                authorization,
                context,
                db,
                cancellationToken),
            "part" => await ResolvePartAsync(normalizedKey, tenantId, authorization, context, db, cancellationToken),
            "purchase_request" => await ResolvePurchaseRequestAsync(
                normalizedKey,
                tenantId,
                authorization,
                context,
                db,
                cancellationToken),
            "purchase_order" => await ResolvePurchaseOrderAsync(
                normalizedKey,
                tenantId,
                authorization,
                context,
                db,
                cancellationToken),
            "receipt" or "receiving_receipt" => await ResolveReceiptAsync(
                normalizedKey,
                tenantId,
                authorization,
                context,
                db,
                cancellationToken),
            "warranty_claim" => await ResolveWarrantyClaimAsync(
                normalizedKey,
                tenantId,
                authorization,
                context,
                db,
                cancellationToken),
            _ => null
        };
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolveSupplierAsync(
        string key,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        authorization.RequireSuppliersRead(context.User);

        var supplier = await db.Suppliers
            .Where(x => x.TenantId == tenantId && x.SupplierKey == key)
            .Select(x => new { x.Id, x.SupplierKey, x.DisplayName })
            .SingleOrDefaultAsync(cancellationToken);

        return supplier is null
            ? null
            : new ExternalReferenceResolutionResponse(
                "supplier",
                supplier.SupplierKey,
                supplier.Id,
                supplier.SupplierKey,
                supplier.DisplayName,
                DateTimeOffset.UtcNow);
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolvePartAsync(
        string key,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        authorization.RequirePartsRead(context.User);

        var part = await db.Parts
            .Where(x => x.TenantId == tenantId && x.PartKey == key)
            .Select(x => new { x.Id, x.PartKey, x.DisplayName })
            .SingleOrDefaultAsync(cancellationToken);

        return part is null
            ? null
            : new ExternalReferenceResolutionResponse(
                "part",
                part.PartKey,
                part.Id,
                part.PartKey,
                part.DisplayName,
                DateTimeOffset.UtcNow);
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolvePurchaseRequestAsync(
        string key,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        authorization.RequirePurchaseRequestRead(context.User);

        var purchaseRequest = await db.PurchaseRequests
            .Where(x => x.TenantId == tenantId && x.RequestKey == key)
            .Select(x => new { x.Id, x.RequestKey, x.Title })
            .SingleOrDefaultAsync(cancellationToken);

        return purchaseRequest is null
            ? null
            : new ExternalReferenceResolutionResponse(
                "purchase_request",
                purchaseRequest.RequestKey,
                purchaseRequest.Id,
                purchaseRequest.RequestKey,
                purchaseRequest.Title,
                DateTimeOffset.UtcNow);
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolvePurchaseOrderAsync(
        string key,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        authorization.RequirePurchaseOrderRead(context.User);

        var purchaseOrder = await db.PurchaseOrders
            .Where(x => x.TenantId == tenantId && x.OrderKey == key)
            .Select(x => new { x.Id, x.OrderKey, x.Title })
            .SingleOrDefaultAsync(cancellationToken);

        return purchaseOrder is null
            ? null
            : new ExternalReferenceResolutionResponse(
                "purchase_order",
                purchaseOrder.OrderKey,
                purchaseOrder.Id,
                purchaseOrder.OrderKey,
                purchaseOrder.Title,
                DateTimeOffset.UtcNow);
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolveReceiptAsync(
        string key,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        authorization.RequireReceivingRead(context.User);

        var receipt = await db.ReceivingReceipts
            .Where(x => x.TenantId == tenantId && x.ReceiptKey == key)
            .Select(x => new { x.Id, x.ReceiptKey })
            .SingleOrDefaultAsync(cancellationToken);

        return receipt is null
            ? null
            : new ExternalReferenceResolutionResponse(
                "receipt",
                receipt.ReceiptKey,
                receipt.Id,
                receipt.ReceiptKey,
                null,
                DateTimeOffset.UtcNow);
    }

    private static async Task<ExternalReferenceResolutionResponse?> ResolveWarrantyClaimAsync(
        string key,
        Guid tenantId,
        SupplyArrAuthorizationService authorization,
        HttpContext context,
        SupplyArrDbContext db,
        CancellationToken cancellationToken)
    {
        authorization.RequirePurchaseOrderRead(context.User);

        var claim = await db.WarrantyClaims
            .Where(x => x.TenantId == tenantId && x.ClaimKey == key)
            .Select(x => new { x.Id, x.ClaimKey, x.ProblemDescription })
            .SingleOrDefaultAsync(cancellationToken);

        return claim is null
            ? null
            : new ExternalReferenceResolutionResponse(
                "warranty_claim",
                claim.ClaimKey,
                claim.Id,
                claim.ClaimKey,
                claim.ProblemDescription,
                DateTimeOffset.UtcNow);
    }

    private static string RouteSuffix(string routePrefix) =>
        routePrefix.Contains("/v1/", StringComparison.OrdinalIgnoreCase) ? "V1" : string.Empty;
}

