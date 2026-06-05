using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionPushEndpoints
{
    public static void MapFieldCompanionPushEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/push", "/api/v1/mobile/push", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var vapid = group.MapGet("/vapid-public-key", (
                IOptions<FieldCompanionWebPushOptions> options) =>
            {
                var settings = options.Value;
                if (!settings.IsConfigured || string.IsNullOrWhiteSpace(settings.PublicKey))
                {
                    throw new StlApiException(
                        "fieldcompanion.push.vapid_unavailable",
                        "Web Push is not configured for this environment.",
                        503);
                }

                return Results.Ok(new FieldCompanionPushVapidPublicKeyResponse(settings.PublicKey));
            });
            if (isCanonical)
            {
                vapid.WithName("GetFieldCompanionPushVapidPublicKey");
            }

            var subscribe = group.MapPost("/subscribe", async (
                [FromBody] UpsertFieldCompanionPushSubscriptionRequest request,
                FieldCompanionPushSubscriptionService subscriptionService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                FieldCompanionFieldInboxService.RequireFieldCompanionAccess(context.User);
                var tenantId = context.User.GetTenantId();
                var userId = context.User.GetUserId();
                var result = await subscriptionService.UpsertAsync(tenantId, userId, request, cancellationToken);
                return Results.Ok(result);
            });
            if (isCanonical)
            {
                subscribe.WithName("SubscribeFieldCompanionPush");
            }

            var unsubscribe = group.MapDelete("/subscribe", async (
                [FromBody] UnsubscribeFieldCompanionPushRequest request,
                FieldCompanionPushSubscriptionService subscriptionService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                FieldCompanionFieldInboxService.RequireFieldCompanionAccess(context.User);
                var tenantId = context.User.GetTenantId();
                var userId = context.User.GetUserId();
                await subscriptionService.UnsubscribeAsync(tenantId, userId, request, cancellationToken);
                return Results.NoContent();
            });
            if (isCanonical)
            {
                unsubscribe.WithName("UnsubscribeFieldCompanionPush");
            }
        });
    }
}
