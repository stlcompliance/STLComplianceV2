using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class VendorPortalEndpoints
{
    public static void MapSupplyArrVendorPortalEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string canonicalRoutePrefix)
        {
            group = group.WithTags("VendorPortal");

            group.MapGet("/rfqs/{rfqId:guid}", async (
                Guid rfqId,
                string accessCode,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.GetVendorPortalAsync(rfqId, accessCode, cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"GetVendorPortalRfq{nameSuffix}");

            group.MapPost("/rfqs/{rfqId:guid}/quotes", async (
                Guid rfqId,
                string accessCode,
                VendorPortalCreateQuoteRequest request,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                var created = await service.CreateVendorPortalQuoteAsync(rfqId, accessCode, request, cancellationToken);
                return Results.Created(
                    $"{canonicalRoutePrefix}/rfqs/{rfqId}/quotes/{created.VendorQuoteId}?accessCode={Uri.EscapeDataString(accessCode)}",
                    created);
            })
            .AllowAnonymous()
            .WithName($"CreateVendorPortalQuote{nameSuffix}");

            group.MapPut("/rfqs/{rfqId:guid}/quotes/{vendorQuoteId:guid}/lines", async (
                Guid rfqId,
                Guid vendorQuoteId,
                string accessCode,
                UpsertVendorQuoteLineRequest request,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.UpsertVendorPortalQuoteLineAsync(
                    rfqId,
                    vendorQuoteId,
                    accessCode,
                    request,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"UpsertVendorPortalQuoteLine{nameSuffix}");

            group.MapPost("/rfqs/{rfqId:guid}/quotes/{vendorQuoteId:guid}/submit", async (
                Guid rfqId,
                Guid vendorQuoteId,
                string accessCode,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.SubmitVendorPortalQuoteAsync(
                    rfqId,
                    vendorQuoteId,
                    accessCode,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"SubmitVendorPortalQuote{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/supplier-portal"), "Supplier", "/api/v1/supplier-portal");
        MapRoutes(app.MapGroup("/api/v1/supplier-portal"), "SupplierV1", "/api/v1/supplier-portal");
        MapRoutes(app.MapGroup("/api/vendor-portal"), string.Empty, "/api/v1/vendor-portal");
        MapRoutes(app.MapGroup("/api/v1/vendor-portal"), "V1", "/api/v1/vendor-portal");
    }
}
