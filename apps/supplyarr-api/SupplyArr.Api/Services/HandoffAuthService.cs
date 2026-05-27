using SupplyArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace SupplyArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    SupplyArrTokenService tokenService)
{
    private const string ProductKey = "supplyarr";

    public async Task<HandoffSessionResponse> RedeemAsync(
        RedeemHandoffRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.HandoffCode))
        {
            throw new StlApiException("handoff.code_missing", "Handoff code is required.", 400);
        }

        var redeemed = await nexArrHandoff.RedeemHandoffAsync(request.HandoffCode, cancellationToken);

        if (!string.Equals(redeemed.TargetProductKey, ProductKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "handoff.product_mismatch",
                "Handoff code is not intended for SupplyArr.",
                403);
        }

        var entitled = redeemed.Entitlements.Contains(ProductKey, StringComparer.OrdinalIgnoreCase);
        if (!entitled)
        {
            throw new StlApiException(
                "handoff.not_entitled",
                "Tenant does not have an active SupplyArr entitlement.",
                403);
        }

        var (accessToken, expiresAt) = tokenService.CreateAccessToken(
            redeemed.UserId,
            redeemed.UserId,
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId,
            redeemed.SessionId,
            redeemed.TenantRoleKey,
            redeemed.Entitlements,
            redeemed.IsPlatformAdmin);

        return new HandoffSessionResponse(
            accessToken,
            expiresAt,
            redeemed.UserId,
            redeemed.UserId,
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId,
            redeemed.TenantSlug,
            redeemed.SessionId,
            redeemed.TenantRoleKey,
            redeemed.IsPlatformAdmin,
            redeemed.Entitlements);
    }
}
