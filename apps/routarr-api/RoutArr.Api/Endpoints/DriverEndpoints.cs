using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DriverEndpoints
{
    public static void MapRoutArrDriverEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/drivers")
            .WithTags("Drivers")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            StaffarrPersonRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            var refs = await service.ListAsync(tenantId, cancellationToken);
            var items = refs.Items
                .Select(x => new DriverResponse(x.PersonId, x.DisplayName, x.MirroredAt))
                .ToList();
            return Results.Ok(new DriverListResponse(items));
        })
        .WithName("ListDrivers");

        group.MapPut("/", async (
            UpsertDriverRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            StaffarrPersonRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var upserted = await service.UpsertAsync(
                tenantId,
                actorUserId,
                new UpsertStaffarrPersonRefRequest(
                    request.PersonId,
                    request.DisplayName,
                    request.SourceUpdatedAt),
                cancellationToken);
            return Results.Ok(new DriverResponse(
                upserted.PersonId,
                upserted.DisplayName,
                upserted.MirroredAt));
        })
        .WithName("UpsertDriver");
    }
}
