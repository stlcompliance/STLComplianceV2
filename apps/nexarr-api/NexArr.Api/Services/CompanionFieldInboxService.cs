using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldInboxService(
    CompanionProductClient productClient,
    CompanionNotificationEnqueueService notificationEnqueueService)
{
    public async Task<AggregatedFieldInboxResponse> GetAsync(
        ClaimsPrincipal principal,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        RequireCompanionAccess(principal);
        var entitlements = principal.GetEntitlements();
        var isPlatformAdmin = principal.IsPlatformAdmin();

        var slices = new List<FieldInboxProductSlice>(FieldInboxRules.FieldProductKeys.Count);
        foreach (var productKey in FieldInboxRules.FieldProductKeys)
        {
            var entitled = isPlatformAdmin || entitlements.Contains(productKey, StringComparer.OrdinalIgnoreCase);
            slices.Add(await productClient.FetchFieldInboxAsync(
                productKey,
                entitled,
                accessToken,
                cancellationToken));
        }

        var response = FieldInboxRules.BuildAggregatedResponse(slices);

        await notificationEnqueueService.TryEnqueueAsync(
            principal.GetTenantId(),
            CompanionNotificationEventKinds.FieldInboxRefreshed,
            principal.GetUserId(),
            "field_inbox",
            principal.GetUserId(),
            cancellationToken);

        return response;
    }

    public static void RequireCompanionAccess(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (principal.HasProductEntitlement("companion"))
        {
            return;
        }

        if (FieldInboxRules.FieldProductKeys.Any(productKey => principal.HasProductEntitlement(productKey)))
        {
            return;
        }

        throw new StlApiException(
            "auth.not_entitled",
            "Field Companion field inbox requires a Field Companion entitlement or a field-product entitlement.",
            403);
    }
}
