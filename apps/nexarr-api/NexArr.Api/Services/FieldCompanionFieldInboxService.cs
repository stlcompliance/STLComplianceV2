using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionFieldInboxService(
    FieldCompanionProductClient productClient,
    FieldCompanionNotificationEnqueueService notificationEnqueueService)
{
    public async Task<AggregatedFieldInboxResponse> GetAsync(
        ClaimsPrincipal principal,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        RequireFieldCompanionAccess(principal);

        var slices = new List<FieldInboxProductSlice>(FieldInboxRules.FieldProductKeys.Count);
        foreach (var productKey in FieldInboxRules.FieldProductKeys)
        {
            slices.Add(await productClient.FetchFieldInboxAsync(
                productKey,
                available: true,
                accessToken,
                cancellationToken));
        }

        var response = FieldInboxRules.BuildAggregatedResponse(slices);

        await notificationEnqueueService.TryEnqueueAsync(
            principal.GetTenantId(),
            FieldCompanionNotificationEventKinds.FieldInboxRefreshed,
            principal.GetUserId(),
            "field_inbox",
            principal.GetUserId(),
            cancellationToken);

        return response;
    }

    public static void RequireFieldCompanionAccess(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }
}
