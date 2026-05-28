using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class IntegrationSettingsEndpoints
{
    public static void MapTrainArrIntegrationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/integration-settings")
            .WithTags("IntegrationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            IntegrationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrIntegrationSettings");

        group.MapPut("/", async (
            UpsertIntegrationSettingsRequest request,
            TrainArrAuthorizationService authorization,
            IntegrationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertTrainArrIntegrationSettings");

        group.MapGet("/probes", async (
            TrainArrAuthorizationService authorization,
            IntegrationProbeService probeService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationSettingsManage(context.User);
            return Results.Ok(await probeService.ProbeAsync(cancellationToken));
        })
        .WithName("GetTrainArrIntegrationProbes");
    }
}
