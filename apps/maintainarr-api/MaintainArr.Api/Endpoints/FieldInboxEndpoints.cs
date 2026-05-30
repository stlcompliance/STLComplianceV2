using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapMaintainArrFieldInboxEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("FieldInbox").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                FieldInboxService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireMaintainArrEntitlement(context.User);
                var tenantId = context.User.GetTenantId();
                var viewAll = authorization.CanViewAllWorkOrders(context.User);
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString();
                return Results.Ok(await service.GetAsync(
                    tenantId,
                    viewAll,
                    actorUserId,
                    actorPersonId,
                    cancellationToken));
            })
            .WithName($"GetMaintainArrFieldInbox{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/field-inbox"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/field-inbox"), "V1");
    }
}
