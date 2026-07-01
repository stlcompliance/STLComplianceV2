using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class SupplierPortalEndpoints
{
    public static void MapSupplyArrSupplierPortalEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string canonicalRoutePrefix)
        {
            group = group.WithTags("SupplierPortal");

            group.MapGet("/rfqs/{rfqId:guid}", async (
                Guid rfqId,
                string accessCode,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.GetSupplierPortalAsync(rfqId, accessCode, cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"GetSupplierPortalRfq{nameSuffix}");

            group.MapPost("/rfqs/{rfqId:guid}/quotes", async (
                Guid rfqId,
                string accessCode,
                SupplierPortalCreateQuoteRequest request,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                var created = await service.CreateSupplierPortalQuoteAsync(rfqId, accessCode, request, cancellationToken);
                return Results.Created(
                    $"{canonicalRoutePrefix}/rfqs/{rfqId}/quotes/{created.SupplierQuoteId}?accessCode={Uri.EscapeDataString(accessCode)}",
                    created);
            })
            .AllowAnonymous()
            .WithName($"CreateSupplierPortalQuote{nameSuffix}");

            group.MapPut("/rfqs/{rfqId:guid}/quotes/{supplierQuoteId:guid}/lines", async (
                Guid rfqId,
                Guid supplierQuoteId,
                string accessCode,
                UpsertSupplierQuoteLineRequest request,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.UpsertSupplierPortalQuoteLineAsync(
                    rfqId,
                    supplierQuoteId,
                    accessCode,
                    request,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"UpsertSupplierPortalQuoteLine{nameSuffix}");

            group.MapPost("/rfqs/{rfqId:guid}/quotes/{supplierQuoteId:guid}/submit", async (
                Guid rfqId,
                Guid supplierQuoteId,
                string accessCode,
                RfqService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.SubmitSupplierPortalQuoteAsync(
                    rfqId,
                    supplierQuoteId,
                    accessCode,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"SubmitSupplierPortalQuote{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-portal"), "SupplierV1", "/api/v1/supplier-portal");
    }
}
