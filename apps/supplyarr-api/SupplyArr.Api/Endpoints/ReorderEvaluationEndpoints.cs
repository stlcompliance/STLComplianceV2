using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ReorderEvaluationEndpoints
{
    public static void MapSupplyArrReorderEvaluationEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("ReorderEvaluation").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReorderEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.EvaluateAsync(tenantId, cancellationToken));
        })
        .WithName($"EvaluateReorderSuggestions{nameSuffix}");

        group.MapGet("/parts/{partId:guid}/policy", async (
            Guid partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReorderEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetPolicyAsync(tenantId, partId, cancellationToken));
        })
        .WithName($"GetPartReorderPolicy{nameSuffix}");

        group.MapPut("/parts/{partId:guid}/policy", async (
            Guid partId,
            UpsertPartReorderPolicyRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReorderEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertPolicyAsync(
                tenantId,
                actorUserId,
                partId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertPartReorderPolicy{nameSuffix}");

        group.MapPost("/create-purchase-request", async (
            CreatePurchaseRequestFromReorderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReorderEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreatePurchaseRequestFromSuggestionsAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"/api/purchase-requests/{created.PurchaseRequestId}", created);
        })
        .WithName($"CreatePurchaseRequestFromReorderSuggestions{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/reorder-evaluation"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/reorder-evaluation"), "V1");
    }
}
