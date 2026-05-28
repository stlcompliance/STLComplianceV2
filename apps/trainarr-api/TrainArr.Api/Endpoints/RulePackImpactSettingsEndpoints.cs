using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class RulePackImpactSettingsEndpoints
{
    public static void MapTrainArrRulePackImpactSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rule-pack-impact-settings")
            .WithTags("RulePackImpactSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            RulePackImpactSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePackImpactSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrRulePackImpactSettings");

        group.MapPut("/", async (
            UpsertRulePackImpactSettingsRequest request,
            TrainArrAuthorizationService authorization,
            RulePackImpactSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePackImpactSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertTrainArrRulePackImpactSettings");

        group.MapGet("/states", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            RulePackImpactWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePackImpactSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentStatesAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrRulePackImpactStates");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            RulePackImpactWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePackImpactSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrRulePackImpactRuns");
    }
}
