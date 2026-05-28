using System.Security.Claims;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionScanResolveService(
    CompanionFieldInboxService fieldInboxService,
    IOptions<CompanionProductUrlsOptions> productUrls)
{
    public async Task<CompanionScanResolveResponse> ResolveAsync(
        ClaimsPrincipal principal,
        string accessToken,
        CompanionScanResolveRequest request,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);

        if (!CompanionScanPayloadParser.TryExtractTaskKey(request.ScannedValue, out var taskKey, out var parseError))
        {
            throw new StlApiException(
                CompanionScanReasonCodes.InvalidPayload,
                parseError ?? "Scan payload is not a recognized field task reference.",
                400);
        }

        if (!CompanionFieldTaskKeyParser.TryParse(taskKey, out var taskRef))
        {
            throw new StlApiException(
                CompanionScanReasonCodes.InvalidPayload,
                "Task key is not a recognized companion field task reference.",
                400);
        }

        if (!IsEntitledToProduct(principal, taskRef.ProductKey))
        {
            return Denied(
                CompanionScanReasonCodes.NotEntitled,
                $"You are not entitled to {taskRef.ProductKey} field tasks.",
                taskKey,
                taskRef.ProductKey);
        }

        var inbox = await fieldInboxService.GetAsync(principal, accessToken, cancellationToken);
        var match = inbox.Items.FirstOrDefault(item =>
            string.Equals(item.TaskKey, taskKey, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return Denied(
                CompanionScanReasonCodes.NotInInbox,
                "This task is not in your field inbox.",
                taskKey,
                taskRef.ProductKey);
        }

        var deepLinkUrl = match.DeepLinkUrl
            ?? BuildDeepLinkUrl(taskRef.ProductKey, match.DeepLinkPath);

        return new CompanionScanResolveResponse(
            CompanionScanOutcomes.Resolved,
            null,
            null,
            match.TaskKey,
            match.ProductKey,
            match.TaskType,
            match.Title,
            match.Subtitle,
            match.Status,
            match.DeepLinkPath,
            deepLinkUrl,
            match.BlockedReason);
    }

    private static bool IsEntitledToProduct(ClaimsPrincipal principal, string productKey)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        return principal.HasProductEntitlement(productKey);
    }

    private string? BuildDeepLinkUrl(string productKey, string deepLinkPath)
    {
        var baseUrl = productKey switch
        {
            "staffarr" => productUrls.Value.StaffArrBaseUrl,
            "trainarr" => productUrls.Value.TrainArrBaseUrl,
            "maintainarr" => productUrls.Value.MaintainArrBaseUrl,
            "routarr" => productUrls.Value.RoutArrBaseUrl,
            "supplyarr" => productUrls.Value.SupplyArrBaseUrl,
            _ => null,
        };

        return FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(baseUrl, deepLinkPath);
    }

    private static CompanionScanResolveResponse Denied(
        string reasonCode,
        string reasonMessage,
        string taskKey,
        string productKey) =>
        new(
            CompanionScanOutcomes.Denied,
            reasonCode,
            reasonMessage,
            taskKey,
            productKey,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
}
