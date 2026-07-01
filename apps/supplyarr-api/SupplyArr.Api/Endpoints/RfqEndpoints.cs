using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class RfqEndpoints
{
    public static void MapSupplyArrRfqEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("Rfqs").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName($"ListRfqs{nameSuffix}");

        group.MapGet("/{rfqId:guid}", async (
            Guid rfqId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, rfqId, cancellationToken));
        })
        .WithName($"GetRfq{nameSuffix}");

        group.MapPost("/", async (
            CreateRfqRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/rfqs/{created.RfqId}", created);
        })
        .WithName($"CreateRfq{nameSuffix}");

        group.MapPut("/{rfqId:guid}", async (
            Guid rfqId,
            UpdateRfqRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(tenantId, actorUserId, rfqId, request, cancellationToken));
        })
        .WithName($"UpdateRfq{nameSuffix}");

        group.MapPost("/{rfqId:guid}/lines", async (
            Guid rfqId,
            AddRfqLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.AddLineAsync(tenantId, actorUserId, rfqId, request, cancellationToken));
        })
        .WithName($"AddRfqLine{nameSuffix}");

        group.MapPut("/{rfqId:guid}/lines/{lineId:guid}", async (
            Guid rfqId,
            Guid lineId,
            UpdateRfqLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLineAsync(tenantId, actorUserId, rfqId, lineId, request, cancellationToken));
        })
        .WithName($"UpdateRfqLine{nameSuffix}");

        group.MapPost("/{rfqId:guid}/submit", async (
            Guid rfqId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SubmitAsync(tenantId, actorUserId, rfqId, cancellationToken));
        })
        .WithName($"SubmitRfq{nameSuffix}");

        group.MapPost("/{rfqId:guid}/invite-suppliers", async (
            Guid rfqId,
            InviteRfqSuppliersRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.InviteSuppliersAsync(tenantId, actorUserId, rfqId, request, cancellationToken));
        })
        .WithName($"InviteRfqSuppliers{nameSuffix}");

        group.MapPost("/{rfqId:guid}/quotes", async (
            Guid rfqId,
            CreateSupplierQuoteRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateSupplierQuoteAsync(tenantId, actorUserId, rfqId, request, cancellationToken);
            return Results.Created($"/api/rfqs/{rfqId}/quotes/{created.SupplierQuoteId}", created);
        })
        .WithName($"CreateSupplierQuote{nameSuffix}");

        group.MapPut("/{rfqId:guid}/quotes/{supplierQuoteId:guid}/lines", async (
            Guid rfqId,
            Guid supplierQuoteId,
            UpsertSupplierQuoteLineRequest request,
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
                rfqId,
                supplierQuoteId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertSupplierQuoteLine{nameSuffix}");

        group.MapPost("/{rfqId:guid}/quotes/{supplierQuoteId:guid}/submit", async (
            Guid rfqId,
            Guid supplierQuoteId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SubmitSupplierQuoteAsync(
                tenantId,
                actorUserId,
                rfqId,
                supplierQuoteId,
                cancellationToken));
        })
        .WithName($"SubmitSupplierQuote{nameSuffix}");

        group.MapGet("/{rfqId:guid}/quote-comparison", async (
            Guid rfqId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.CompareQuotesAsync(tenantId, rfqId, cancellationToken));
        })
        .WithName($"CompareRfqQuotes{nameSuffix}");

        group.MapPost("/{rfqId:guid}/select-quote", async (
            Guid rfqId,
            SelectSupplierQuoteRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqAward(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SelectSupplierQuoteAsync(
                tenantId,
                actorUserId,
                rfqId,
                request,
                cancellationToken));
        })
        .WithName($"SelectRfqSupplierQuote{nameSuffix}");

        group.MapPost("/{rfqId:guid}/create-purchase-request", async (
            Guid rfqId,
            CreatePurchaseRequestFromRfqRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            RfqService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRfqManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreatePurchaseRequestFromRfqAsync(
                tenantId,
                actorUserId,
                rfqId,
                request,
                cancellationToken);
            return Results.Created($"/api/purchase-requests/{created.PurchaseRequestId}", created);
        })
        .WithName($"CreatePurchaseRequestFromRfq{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/rfqs"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/rfqs"), "V1");
    }
}
