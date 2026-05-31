using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ForgivingSearchEndpoints
{
    public static void MapSupplyArrForgivingSearchEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("ForgivingSearch").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                SupplyArrAuthorizationService authorization) =>
            {
                authorization.RequireForgivingSearch(context.User);
                return Results.Ok(new
                {
                    items = new[]
                    {
                        new { key = "forgiving", path = "/api/v1/search/forgiving" },
                    }
                });
            })
            .WithName($"GetSupplyArrSearchIndex{nameSuffix}");

            group.MapGet("/forgiving", async (
                string q,
                int? limit,
                SupplyArrAuthorizationService authorization,
                ForgivingSearchService searchService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireForgivingSearch(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await searchService.SearchAsync(tenantId, q, limit, cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.search.forgiving",
                    tenantId,
                    actorUserId,
                    "forgiving_search",
                    null,
                    "success",
                    reasonCode: $"result_count:{response.TotalCount}",
                    cancellationToken: cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"GetSupplyArrForgivingSearch{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/search"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/search"), "V1");
    }
}
