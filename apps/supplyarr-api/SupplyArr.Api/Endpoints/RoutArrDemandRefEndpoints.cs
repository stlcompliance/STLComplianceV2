using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class RoutArrDemandRefEndpoints
{
    public static void MapSupplyArrRoutArrDemandRefEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("RoutArrDemandRefs").RequireAuthorization();

            group.MapGet("/", async (
                string? status,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                RoutArrDemandIntakeService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDemandRefRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
            })
            .WithName($"ListRoutArrDemandRefs{nameSuffix}");

            group.MapGet("/{demandRefId:guid}", async (
                Guid demandRefId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                RoutArrDemandIntakeService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDemandRefRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetAsync(tenantId, demandRefId, cancellationToken));
            })
            .WithName($"GetRoutArrDemandRef{nameSuffix}");

            group.MapPost("/{demandRefId:guid}/create-purchase-request", async (
                Guid demandRefId,
                CreatePurchaseRequestFromRoutarrDemandRefRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                RoutArrDemandIntakeService service,
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
            .WithName($"CreatePurchaseRequestFromRoutArrDemandRef{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/routarr-demand-refs"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/routarr-demand-refs"), "V1");
    }
}
