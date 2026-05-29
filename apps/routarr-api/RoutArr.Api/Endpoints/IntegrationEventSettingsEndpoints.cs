using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class IntegrationEventSettingsEndpoints
{
    public static void MapRoutArrIntegrationEventSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/integration-event-settings")
            .WithTags("IntegrationEventSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            RoutArrAuthorizationService authorization,
            IntegrationEventSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetRoutArrIntegrationEventSettings");

        group.MapPut("/", async (
            UpsertIntegrationEventSettingsRequest request,
            RoutArrAuthorizationService authorization,
            IntegrationEventSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertRoutArrIntegrationEventSettings");

        group.MapGet("/outbox", async (
            int? limit,
            RoutArrAuthorizationService authorization,
            IntegrationEventProcessingService processingService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await processingService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListRoutArrIntegrationOutboxEvents");
    }
}
