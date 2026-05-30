using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class DemandRefEndpoints
{
    public static void MapSupplyArrDemandRefEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("DemandRefs").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            MaintainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandRefRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName($"ListDemandRefs{nameSuffix}");

        group.MapGet("/{demandRefId:guid}", async (
            Guid demandRefId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            MaintainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandRefRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, demandRefId, cancellationToken));
        })
        .WithName($"GetDemandRef{nameSuffix}");

        group.MapPost("/{demandRefId:guid}/create-purchase-request", async (
            Guid demandRefId,
            CreatePurchaseRequestFromDemandRefRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            MaintainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreatePurchaseRequestFromDemandRefAsync(
                tenantId,
                actorUserId,
                demandRefId,
                request,
                cancellationToken);
            return Results.Created($"/api/purchase-requests/{created.PurchaseRequestId}", created);
        })
        .WithName($"CreatePurchaseRequestFromDemandRef{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/demand-refs"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/demand-refs"), "V1");
    }
}
