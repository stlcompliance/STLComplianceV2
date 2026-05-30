using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class DemandProcessingEndpoints
{
    public static void MapSupplyArrDemandProcessingEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("DemandProcessing").RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            DemandProcessingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDashboardAsync(tenantId, cancellationToken));
        })
        .WithName($"GetSupplyArrDemandProcessingDashboard{nameSuffix}");

        group.MapGet("/{demandRefId:guid}", async (
            Guid demandRefId,
            SupplyArrAuthorizationService authorization,
            DemandProcessingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDetailAsync(tenantId, demandRefId, cancellationToken));
        })
        .WithName($"GetSupplyArrDemandProcessingDetail{nameSuffix}");

        group.MapPost("/{demandRefId:guid}/retry-processing", async (
            Guid demandRefId,
            SupplyArrAuthorizationService authorization,
            DemandProcessingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingOperate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RetryProcessingAsync(
                tenantId,
                actorUserId,
                demandRefId,
                cancellationToken));
        })
        .WithName($"RetrySupplyArrDemandProcessing{nameSuffix}");

        group.MapPost("/{demandRefId:guid}/create-pr-draft", async (
            Guid demandRefId,
            SupplyArrAuthorizationService authorization,
            DemandProcessingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingOperate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreatePurchaseRequestDraftAsync(
                tenantId,
                actorUserId,
                demandRefId,
                cancellationToken));
        })
        .WithName($"CreateSupplyArrDemandProcessingPrDraft{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/demand-processing"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/demand-processing"), "V1");
    }
}
