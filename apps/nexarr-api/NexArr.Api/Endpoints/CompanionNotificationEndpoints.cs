using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class CompanionNotificationEndpoints
{
    public static void MapCompanionNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/notification-settings")
            .WithTags("CompanionNotifications")
            .RequireAuthorization();

        group.MapGet("/", async (
            CompanionNotificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            CompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetCompanionNotificationSettings");

        group.MapPut("/", async (
            UpsertCompanionNotificationSettingsRequest request,
            CompanionNotificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            CompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertCompanionNotificationSettings");

        group.MapGet("/dispatches", async (
            int? limit,
            CompanionNotificationDispatchService dispatchService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            CompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListCompanionNotificationDispatches");
    }
}
