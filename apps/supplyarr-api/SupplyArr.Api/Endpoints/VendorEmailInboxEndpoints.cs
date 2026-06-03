using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorEmailInboxEndpoints
{
    public static void MapSupplyArrVendorEmailInboxEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("VendorEmailInbox").RequireAuthorization();

            group.MapGet("/", async (
                int? limit,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorEmailInboxService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireRfqRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListVendorEmailInbox{nameSuffix}");

            group.MapPost("/", async (
                IngestVendorEmailInboxRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorEmailInboxService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireRfqManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.IngestAsync(tenantId, actorUserId, request, cancellationToken));
            })
            .WithName($"IngestVendorEmailInbox{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/vendor-email-inbox"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/vendor-email-inbox"), "V1");
    }
}
