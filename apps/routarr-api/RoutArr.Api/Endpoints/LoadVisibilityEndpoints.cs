using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class LoadVisibilityEndpoints
{
    public static void MapRoutArrLoadVisibilityEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/load-visibility"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/load-visibility"), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group
            .WithTags("LoadVisibility")
            .RequireAuthorization()
            .MapGet("/", async (
                Guid? tripId,
                HttpContext context,
                RoutArrAuthorizationService authorization,
                LoadVisibilityService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireTripsRead(context.User);
                var viewAll = authorization.CanViewAllTrips(context.User);
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString();
                return Results.Ok(await service.ListAsync(
                    context.User,
                    viewAll,
                    actorUserId,
                    actorPersonId,
                    tripId,
                    cancellationToken));
            })
            .WithName($"ListLoadVisibility{nameSuffix}");
    }
}
