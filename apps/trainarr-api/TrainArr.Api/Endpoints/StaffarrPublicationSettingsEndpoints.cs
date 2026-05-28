using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class StaffarrPublicationSettingsEndpoints
{
    public static void MapTrainArrStaffarrPublicationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/staffarr-publication-settings")
            .WithTags("StaffarrPublicationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            StaffarrPublicationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffarrPublicationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrStaffarrPublicationSettings");

        group.MapPut("/", async (
            UpsertStaffarrPublicationSettingsRequest request,
            TrainArrAuthorizationService authorization,
            StaffarrPublicationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffarrPublicationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertTrainArrStaffarrPublicationSettings");

        group.MapGet("/deliveries", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            StaffarrPublicationRetryService retryService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffarrPublicationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await retryService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrStaffarrPublicationDeliveries");
    }
}
