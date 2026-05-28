using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class IntegrationEventSettingsEndpoints
{
    public static void MapSupplyArrIntegrationEventSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/integration-event-settings")
            .WithTags("IntegrationEventSettings")
            .RequireAuthorization();

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
        .WithName("GetSupplyArrIntegrationEventSettings");

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
        .WithName("UpsertSupplyArrIntegrationEventSettings");

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
        .WithName("ListSupplyArrIntegrationOutboxEvents");

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
        .WithName("ListSupplyArrIntegrationInboxEvents");

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
        .WithName("AbandonSupplyArrIntegrationOutboxEvent");

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
        .WithName("AbandonSupplyArrIntegrationInboxEvent");
    }
}
