using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class VehicleRefEndpoints
{
    public static void MapRoutArrVehicleRefEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vehicle-refs")
            .WithTags("VehicleRefs")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            VehicleRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListVehicleRefs");

        group.MapPut("/", async (
            UpsertVehicleRefRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            VehicleRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertVehicleRef");
    }
}
