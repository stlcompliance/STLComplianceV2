using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class RecertificationSettingsEndpoints
{
    public static void MapTrainArrRecertificationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recertification-settings")
            .WithTags("RecertificationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            RecertificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRecertificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrRecertificationSettings");

        group.MapPut("/", async (
            UpsertRecertificationSettingsRequest request,
            TrainArrAuthorizationService authorization,
            RecertificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRecertificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertTrainArrRecertificationSettings");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            RecertificationAssignmentService assignmentService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRecertificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await assignmentService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrRecertificationAssignmentRuns");
    }
}
