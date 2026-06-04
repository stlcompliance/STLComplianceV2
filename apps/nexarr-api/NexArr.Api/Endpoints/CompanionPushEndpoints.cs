using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class CompanionPushEndpoints
{
    public static void MapCompanionPushEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/companion/push", "/api/v1/mobile/push", (group, isCanonical) =>
        {
            group.WithTags("FieldCompanion").RequireAuthorization();

            var vapid = group.MapGet("/vapid-public-key", (
                IOptions<CompanionWebPushOptions> options) =>
            {
                var settings = options.Value;
                if (!settings.IsConfigured || string.IsNullOrWhiteSpace(settings.PublicKey))
                {
                    throw new StlApiException(
                        "companion.push.vapid_unavailable",
                        "Web Push is not configured for this environment.",
                        503);
                }

                return Results.Ok(new CompanionPushVapidPublicKeyResponse(settings.PublicKey));
            });
            if (isCanonical)
            {
                vapid.WithName("GetCompanionPushVapidPublicKey");
            }

            var subscribe = group.MapPost("/subscribe", async (
                [FromBody] UpsertCompanionPushSubscriptionRequest request,
                CompanionPushSubscriptionService subscriptionService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                CompanionFieldInboxService.RequireCompanionAccess(context.User);
                var tenantId = context.User.GetTenantId();
                var userId = context.User.GetUserId();
                var result = await subscriptionService.UpsertAsync(tenantId, userId, request, cancellationToken);
                return Results.Ok(result);
            });
            if (isCanonical)
            {
                subscribe.WithName("SubscribeCompanionPush");
            }

            var unsubscribe = group.MapDelete("/subscribe", async (
                [FromBody] UnsubscribeCompanionPushRequest request,
                CompanionPushSubscriptionService subscriptionService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                CompanionFieldInboxService.RequireCompanionAccess(context.User);
                var tenantId = context.User.GetTenantId();
                var userId = context.User.GetUserId();
                await subscriptionService.UnsubscribeAsync(tenantId, userId, request, cancellationToken);
                return Results.NoContent();
            });
            if (isCanonical)
            {
                unsubscribe.WithName("UnsubscribeCompanionPush");
            }
        });
    }
}
