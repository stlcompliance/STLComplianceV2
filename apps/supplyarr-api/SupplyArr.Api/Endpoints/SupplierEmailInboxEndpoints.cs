using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierEmailInboxEndpoints
{
    public static void MapSupplyArrSupplierEmailInboxEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("SupplierEmailInbox").RequireAuthorization();

            group.MapGet("/", async (
                int? limit,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierEmailInboxService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireRfqRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListSupplierEmailInbox{nameSuffix}");

            group.MapPost("/", async (
                IngestSupplierEmailInboxRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierEmailInboxService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireRfqManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.IngestAsync(tenantId, actorUserId, request, cancellationToken));
            })
            .WithName($"IngestSupplierEmailInbox{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-email-inbox"), "V1");
    }
}
