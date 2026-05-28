using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class TrainArrDemandRefEndpoints
{
    public static void MapSupplyArrTrainArrDemandRefEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trainarr-demand-refs").WithTags("TrainArrDemandRefs").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            TrainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandRefRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName("ListTrainArrDemandRefs");

        group.MapGet("/{demandRefId:guid}", async (
            Guid demandRefId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            TrainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandRefRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, demandRefId, cancellationToken));
        })
        .WithName("GetTrainArrDemandRef");

        group.MapPost("/{demandRefId:guid}/create-purchase-request", async (
            Guid demandRefId,
            CreatePurchaseRequestFromTrainarrDemandRefRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            TrainArrDemandIntakeService service,
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
        .WithName("CreatePurchaseRequestFromTrainArrDemandRef");
    }
}
