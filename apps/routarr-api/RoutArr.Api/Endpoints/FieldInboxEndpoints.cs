using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapRoutArrFieldInboxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/field-inbox")
            .WithTags("FieldInbox")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            FieldInboxService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.GetAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                cancellationToken));
        })
        .WithName("GetRoutArrFieldInbox");
    }
}
