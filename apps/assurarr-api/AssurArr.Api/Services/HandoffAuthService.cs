using AssurArr.Api.Contracts;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace AssurArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    AssurArrTokenService tokenService)
{
    private const string ProductKey = "assurarr";

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
                "Handoff code is not intended for AssurArr.",
                403);
        }

        var entitled = redeemed.Entitlements.Contains(ProductKey, StringComparer.OrdinalIgnoreCase);
        if (!entitled)
        {
            throw new StlApiException(
                "handoff.not_entitled",
                "Tenant does not have an active AssurArr entitlement.",
                403);
        }

        var personId = redeemed.UserId;
        var (accessToken, expiresAt) = tokenService.CreateAccessToken(
            redeemed.UserId,
            personId,
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId,
            redeemed.SessionId,
            redeemed.TenantRoleKey,
            redeemed.Entitlements,
            redeemed.IsPlatformAdmin,
            redeemed.AccessTokenMinutes);

        return new HandoffSessionResponse(
            accessToken,
            expiresAt,
            redeemed.UserId.ToString(),
            personId.ToString(),
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId.ToString(),
            redeemed.TenantSlug,
            redeemed.TenantDisplayName,
            redeemed.SessionId.ToString(),
            redeemed.TenantRoleKey,
            redeemed.IsPlatformAdmin,
            redeemed.Entitlements);
    }
}
