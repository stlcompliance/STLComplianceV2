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
        var group = app.MapGroup("/api/companion/push")
            .WithTags("CompanionPush")
            .RequireAuthorization();

        group.MapGet("/vapid-public-key", (
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
        })
        .WithName("GetCompanionPushVapidPublicKey");

        group.MapPost("/subscribe", async (
            UpsertCompanionPushSubscriptionRequest request,
            CompanionPushSubscriptionService subscriptionService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            CompanionFieldInboxService.RequireCompanionAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var userId = context.User.GetUserId();
            var result = await subscriptionService.UpsertAsync(tenantId, userId, request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("SubscribeCompanionPush");

        group.MapDelete("/subscribe", async (
            UnsubscribeCompanionPushRequest request,
            CompanionPushSubscriptionService subscriptionService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            CompanionFieldInboxService.RequireCompanionAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var userId = context.User.GetUserId();
            await subscriptionService.UnsubscribeAsync(tenantId, userId, request, cancellationToken);
            return Results.NoContent();
        })
        .WithName("UnsubscribeCompanionPush");
    }
}
