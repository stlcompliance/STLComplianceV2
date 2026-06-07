using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DockAppointmentEndpoints
{
    public static void MapRoutArrDockAppointmentEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/dock-appointments"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/dock-appointments"), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group
            .WithTags("DockAppointments")
            .RequireAuthorization()
            .MapGet("/", async (
                Guid? tripId,
                HttpContext context,
                RoutArrAuthorizationService authorization,
                DockAppointmentService service,
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
            .WithName($"ListDockAppointments{nameSuffix}");
    }
}
