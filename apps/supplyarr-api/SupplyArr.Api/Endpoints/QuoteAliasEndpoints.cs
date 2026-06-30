using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class QuoteAliasEndpoints
{
    public static void MapSupplyArrQuoteAliasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/quotes").WithTags("Rfqs").RequireAuthorization();

        group.MapGet("/", async (
            Guid rfqId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqRead(context.User);
            var tenantId = context.User.GetTenantId();
            var rfq = await service.GetAsync(tenantId, rfqId, cancellationToken);
            return Results.Ok(rfq.Quotes);
        })
        .WithName("ListQuotesV1");

        group.MapGet("/{vendorQuoteId:guid}", async (
            Guid vendorQuoteId,
            Guid rfqId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqRead(context.User);
            var tenantId = context.User.GetTenantId();
            var rfq = await service.GetAsync(tenantId, rfqId, cancellationToken);
            var quote = rfq.Quotes.FirstOrDefault(x => x.VendorQuoteId == vendorQuoteId);
            return quote is null ? Results.NotFound() : Results.Ok(quote);
        })
        .WithName("GetQuoteV1");

        group.MapPost("/", async (
            CreateQuoteRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateVendorQuoteAsync(
                tenantId,
                actorUserId,
                request.RfqId,
                new CreateVendorQuoteRequest(
                    request.SupplierId,
                    request.VendorPartyId,
                    request.QuoteKey,
                    request.CurrencyCode,
                    request.Notes),
                cancellationToken);
            return Results.Created($"/api/v1/quotes/{created.VendorQuoteId}?rfqId={request.RfqId}", created);
        })
        .WithName("CreateQuoteV1");

        group.MapPut("/{vendorQuoteId:guid}/lines", async (
            Guid vendorQuoteId,
            UpsertQuoteLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertQuoteLineAsync(
                tenantId,
                actorUserId,
                request.RfqId,
                vendorQuoteId,
                new UpsertVendorQuoteLineRequest(
                    request.RfqLineId,
                    request.UnitPrice,
                    request.QuantityQuoted,
                    request.LeadTimeDays,
                    request.Notes),
                cancellationToken));
        })
        .WithName("UpsertQuoteLineV1");

        group.MapPost("/{vendorQuoteId:guid}/submit", async (
            Guid vendorQuoteId,
            Guid rfqId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SubmitVendorQuoteAsync(
                tenantId,
                actorUserId,
                rfqId,
                vendorQuoteId,
                cancellationToken));
        })
        .WithName("SubmitQuoteV1");
    }
}
