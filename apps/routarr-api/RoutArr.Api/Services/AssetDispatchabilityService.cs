using Microsoft.Extensions.Options;
using RoutArr.Api.Contracts;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class AssetDispatchabilityService(
    MaintainArrAssetReadinessClient maintainArrClient,
    IOptions<AssetDispatchabilityOptions> dispatchabilityOptions,
    IRoutArrAuditService audit)
{
    public const string CheckAction = "asset_dispatchability.check";

    public async Task<AssetDispatchabilityCheckResponse> CheckAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? vehicleRefKey,
        string? assetTag,
        CancellationToken cancellationToken = default)
    {
        var normalizedVehicleRef = NormalizeOptionalKey(vehicleRefKey);
        var normalizedAssetTag = NormalizeOptionalKey(assetTag);

        if (string.IsNullOrWhiteSpace(normalizedVehicleRef) && string.IsNullOrWhiteSpace(normalizedAssetTag))
        {
            throw new StlApiException(
                "asset_dispatchability.ref_required",
                "Vehicle reference key or asset tag is required.",
                400);
        }

        var options = dispatchabilityOptions.Value;
        AssetDispatchabilityMaintainArrSummary? maintainArrSummary = null;
        var assetFound = false;

        if (options.CheckMaintainArrReadiness && maintainArrClient.IsConfigured)
        {
            var maintainArrResult = await maintainArrClient.GetReadinessAsync(
                tenantId,
                normalizedVehicleRef,
                normalizedAssetTag,
                cancellationToken);

            if (maintainArrResult is not null)
            {
                assetFound = true;
                maintainArrSummary = new AssetDispatchabilityMaintainArrSummary(
                    maintainArrResult.AssetId,
                    maintainArrResult.AssetTag,
                    maintainArrResult.ReadinessStatus,
                    maintainArrResult.ReadinessBasis,
                    maintainArrResult.Blockers.Count,
                    maintainArrResult.Blockers.FirstOrDefault()?.Message);
            }
        }

        var outcome = maintainArrSummary is null && !maintainArrClient.IsConfigured
            ? AssetDispatchabilityOutcomes.Warn
            : AssetDispatchabilityRules.MergeOutcome(maintainArrSummary?.ReadinessStatus, assetFound);

        if (maintainArrSummary is null && maintainArrClient.IsConfigured && !assetFound)
        {
            outcome = AssetDispatchabilityOutcomes.Warn;
        }

        var (reasonCode, message) = maintainArrSummary is null && !maintainArrClient.IsConfigured
            ? ("dispatchability_check_unavailable", "Asset dispatchability integrations are not configured.")
            : AssetDispatchabilityRules.BuildMergedReason(outcome, maintainArrSummary, assetFound);

        var response = new AssetDispatchabilityCheckResponse(
            normalizedVehicleRef,
            normalizedAssetTag,
            outcome,
            reasonCode,
            message,
            AssetDispatchabilityRules.IsBlockingOutcome(outcome),
            maintainArrSummary);

        if (actorUserId.HasValue)
        {
            var resourceKey = normalizedAssetTag ?? normalizedVehicleRef ?? string.Empty;
            await audit.WriteAsync(
                CheckAction,
                tenantId,
                actorUserId.Value,
                "asset",
                resourceKey,
                outcome,
                cancellationToken: cancellationToken);
        }

        return response;
    }

    public async Task EnsureAssetDispatchableAsync(
        Guid tenantId,
        string? vehicleRefKey,
        string? assetTag,
        bool ignoreDispatchabilityBlocks,
        CancellationToken cancellationToken = default)
    {
        if (ignoreDispatchabilityBlocks)
        {
            return;
        }

        var normalizedVehicleRef = NormalizeOptionalKey(vehicleRefKey);
        var normalizedAssetTag = NormalizeOptionalKey(assetTag);
        if (string.IsNullOrWhiteSpace(normalizedVehicleRef) && string.IsNullOrWhiteSpace(normalizedAssetTag))
        {
            return;
        }

        var dispatchability = await CheckAsync(
            tenantId,
            actorUserId: null,
            normalizedVehicleRef,
            normalizedAssetTag,
            cancellationToken);

        if (dispatchability.IsBlocking)
        {
            throw new StlApiException(
                "dispatch.asset_dispatchability_blocked",
                dispatchability.Message,
                409,
                dispatchability);
        }
    }

    private static string? NormalizeOptionalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "asset_dispatchability.ref_too_long",
                "Vehicle reference key or asset tag must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }
}
