using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class QualificationRecalculationSettingsEndpoints
{
    public static void MapTrainArrQualificationRecalculationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/qualification-recalculation-settings")
            .WithTags("QualificationRecalculationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            QualificationRecalculationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationRecalculationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrQualificationRecalculationSettings");

        group.MapPut("/", async (
            UpsertQualificationRecalculationSettingsRequest request,
            TrainArrAuthorizationService authorization,
            QualificationRecalculationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationRecalculationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertTrainArrQualificationRecalculationSettings");

        group.MapGet("/states", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            QualificationRecalculationService recalculationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationRecalculationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await recalculationService.ListRecentStatesAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrQualificationRecalculationStates");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            QualificationRecalculationService recalculationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationRecalculationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await recalculationService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrQualificationRecalculationRuns");
    }
}
