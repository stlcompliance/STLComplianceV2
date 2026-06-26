using CustomArr.Api.Data;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    CustomArrTokenService tokenService)
{
    private const string ProductKey = "customarr";

    public async Task<CustomArrHandoffSessionResponse> RedeemAsync(
        StlNexArrRedeemHandoffRequest request,
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
                "Handoff code is not intended for CustomArr.",
                403);
        }

        if (!redeemed.LaunchableProductKeys.Contains(ProductKey, StringComparer.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "handoff.not_available",
                "CustomArr is not available for this tenant.",
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
            redeemed.LaunchableProductKeys,
            redeemed.IsPlatformAdmin,
            redeemed.AccessTokenMinutes);

        return new CustomArrHandoffSessionResponse(
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
            redeemed.LaunchableProductKeys,
            redeemed.ThemePreference,
            redeemed.CallbackUrl);
    }
}
