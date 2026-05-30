using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class IntegrationEventSettingsEndpoints
{
    public static void MapSupplyArrIntegrationEventSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("IntegrationEventSettings").RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            IntegrationEventSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName($"GetSupplyArrIntegrationEventSettings{nameSuffix}");

        group.MapPut("/", async (
            UpsertIntegrationEventSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            IntegrationEventSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName($"UpsertSupplyArrIntegrationEventSettings{nameSuffix}");

        group.MapGet("/outbox", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            IntegrationEventProcessingService processingService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await processingService.ListRecentOutboxAsync(tenantId, limit, cancellationToken));
        })
        .WithName($"ListSupplyArrIntegrationOutboxEvents{nameSuffix}");

        group.MapGet("/inbox", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            IntegrationEventProcessingService processingService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await processingService.ListRecentInboxAsync(tenantId, limit, cancellationToken));
        })
        .WithName($"ListSupplyArrIntegrationInboxEvents{nameSuffix}");

        group.MapPost("/outbox/{eventId:guid}/abandon", async (
            Guid eventId,
            SupplyArrAuthorizationService authorization,
            IntegrationEventProcessingService processingService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await processingService.AbandonOutboxAsync(tenantId, eventId, actorUserId, cancellationToken));
        })
        .WithName($"AbandonSupplyArrIntegrationOutboxEvent{nameSuffix}");

        group.MapPost("/inbox/{eventId:guid}/abandon", async (
            Guid eventId,
            SupplyArrAuthorizationService authorization,
            IntegrationEventProcessingService processingService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await processingService.AbandonInboxAsync(tenantId, eventId, actorUserId, cancellationToken));
        })
        .WithName($"AbandonSupplyArrIntegrationInboxEvent{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/integration-event-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integration-event-settings"), "V1");
    }
}
