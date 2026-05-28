using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class EvidenceRetentionSettingsEndpoints
{
    public static void MapTrainArrEvidenceRetentionSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/evidence-retention-settings")
            .WithTags("EvidenceRetentionSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            EvidenceRetentionSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEvidenceRetentionSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrEvidenceRetentionSettings");

        group.MapPut("/", async (
            UpsertEvidenceRetentionSettingsRequest request,
            TrainArrAuthorizationService authorization,
            EvidenceRetentionSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEvidenceRetentionSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertTrainArrEvidenceRetentionSettings");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            EvidenceRetentionWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEvidenceRetentionSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrEvidenceRetentionRuns");
    }
}
