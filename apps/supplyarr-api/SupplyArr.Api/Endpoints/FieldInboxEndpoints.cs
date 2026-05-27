using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapSupplyArrFieldInboxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/field-inbox")
            .WithTags("FieldInbox")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            FieldInboxService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var viewAll = context.User.IsPlatformAdmin()
                || MatchesReceivingManagerRole(context.User.GetTenantRoleKey());
            return Results.Ok(await service.GetAsync(tenantId, actorUserId, viewAll, cancellationToken));
        })
        .WithName("GetSupplyArrFieldInbox");
    }

    private static bool MatchesReceivingManagerRole(string roleKey) =>
        roleKey.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
        || roleKey.Equals("supplyarr_admin", StringComparison.OrdinalIgnoreCase)
        || roleKey.Equals("warehouse_manager", StringComparison.OrdinalIgnoreCase);
}
