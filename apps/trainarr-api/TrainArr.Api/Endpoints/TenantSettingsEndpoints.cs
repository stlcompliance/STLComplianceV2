using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TenantSettingsEndpoints
{
    public static void MapTrainArrTenantSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/tenant-settings/trainarr")
            .WithTags("TenantSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            TrainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            return Results.Ok(await settingsService.GetOrCreateAsync(
                context.User.GetTenantId(),
                cancellationToken));
        })
        .WithName("GetTrainArrTenantSettings");

        group.MapGet("/defaults", (
            TrainArrAuthorizationService authorization,
            TrainArrTenantSettingsService settingsService,
            HttpContext context) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            return Results.Ok(settingsService.GetDefaults());
        })
        .WithName("GetTrainArrTenantSettingsDefaults");

        group.MapPut("/", async (
            UpdateTrainArrTenantSettingsRequest request,
            TrainArrAuthorizationService authorization,
            TrainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsManage(context.User);
            return Results.Ok(await settingsService.PutAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId(),
                request,
                cancellationToken));
        })
        .WithName("PutTrainArrTenantSettings");

        group.MapPatch("/", async (
            PatchTrainArrTenantSettingsRequest request,
            TrainArrAuthorizationService authorization,
            TrainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsManage(context.User);
            return Results.Ok(await settingsService.PatchAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId(),
                request,
                cancellationToken));
        })
        .WithName("PatchTrainArrTenantSettings");
    }
}
