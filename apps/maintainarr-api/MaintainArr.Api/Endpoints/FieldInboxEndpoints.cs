using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapMaintainArrFieldInboxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/field-inbox")
            .WithTags("FieldInbox")
            .RequireAuthorization();

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
        .WithName("GetMaintainArrFieldInbox");
    }
}
