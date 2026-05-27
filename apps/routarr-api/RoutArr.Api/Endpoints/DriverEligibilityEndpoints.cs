using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DriverEligibilityEndpoints
{
    public static void MapRoutArrDriverEligibilityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/driver-eligibility")
            .WithTags("DriverEligibility")
            .RequireAuthorization();

        group.MapPost("/check", async (
            DriverEligibilityCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverEligibilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.CheckAsync(
                tenantId,
                actorUserId,
                request.PersonId,
                request.QualificationKey,
                request.RulePackKey,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CheckDriverEligibility");
    }
}
