using RoutArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace RoutArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    RoutArrTokenService tokenService)
{
    private const string ProductKey = "routarr";

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
                "Handoff code is not intended for RoutArr.",
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
            redeemed.LaunchableProductKeys,
            redeemed.IsPlatformAdmin,
            redeemed.AccessTokenMinutes);

        return new HandoffSessionResponse(
            accessToken,
            expiresAt,
            redeemed.UserId,
            redeemed.UserId,
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId,
            redeemed.TenantSlug,
            redeemed.TenantDisplayName,
            redeemed.SessionId,
            redeemed.TenantRoleKey,
            redeemed.IsPlatformAdmin,
            redeemed.LaunchableProductKeys,
            redeemed.ThemePreference,
            redeemed.CallbackUrl);
    }
}
