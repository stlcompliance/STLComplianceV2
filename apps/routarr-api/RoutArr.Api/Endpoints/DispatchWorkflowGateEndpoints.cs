using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DispatchWorkflowGateEndpoints
{
    public static void MapRoutArrDispatchWorkflowGateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dispatch-workflow-gates")
            .WithTags("DispatchWorkflowGates")
            .RequireAuthorization();

        group.MapPost("/check", async (
            DispatchWorkflowGateCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchWorkflowGateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.CheckAsync(
                tenantId,
                actorUserId,
                request.TripId,
                request.DriverPersonId,
                request.VehicleRefKey,
                request.AssignmentKind,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CheckDispatchWorkflowGates");
    }
}
