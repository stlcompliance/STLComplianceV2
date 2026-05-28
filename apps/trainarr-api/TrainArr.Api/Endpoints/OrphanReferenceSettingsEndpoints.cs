using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class OrphanReferenceSettingsEndpoints
{
    public static void MapTrainArrOrphanReferenceSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orphan-reference-settings")
            .WithTags("OrphanReferenceSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            OrphanReferenceSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOrphanReferenceSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrOrphanReferenceSettings");

        group.MapPut("/", async (
            UpsertOrphanReferenceSettingsRequest request,
            TrainArrAuthorizationService authorization,
            OrphanReferenceSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOrphanReferenceSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertTrainArrOrphanReferenceSettings");

        group.MapGet("/findings", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            OrphanReferenceWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOrphanReferenceSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListActiveFindingsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrOrphanReferenceFindings");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            OrphanReferenceWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOrphanReferenceSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrOrphanReferenceRuns");
    }
}
