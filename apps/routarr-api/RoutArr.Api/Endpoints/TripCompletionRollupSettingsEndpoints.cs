using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripCompletionRollupSettingsEndpoints
{
    public static void MapRoutArrTripCompletionRollupSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trip-completion-rollup-settings")
            .WithTags("TripCompletionRollupSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            RoutArrAuthorizationService authorization,
            TripCompletionRollupSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripCompletionRollupSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetRoutArrTripCompletionRollupSettings");

        group.MapPut("/", async (
            UpsertTripCompletionRollupSettingsRequest request,
            RoutArrAuthorizationService authorization,
            TripCompletionRollupSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripCompletionRollupSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertRoutArrTripCompletionRollupSettings");

        group.MapGet("/pending", async (
            RoutArrAuthorizationService authorization,
            TripCompletionRollupWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripCompletionRollupSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListRoutArrPendingTripCompletionRollups");

        group.MapGet("/runs", async (
            int? limit,
            RoutArrAuthorizationService authorization,
            TripCompletionRollupWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripCompletionRollupSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListRoutArrTripCompletionRollupRuns");
    }
}
