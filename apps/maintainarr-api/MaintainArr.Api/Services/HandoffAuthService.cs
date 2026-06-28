using MaintainArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace MaintainArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    MaintainArrTokenService tokenService)
{
    private const string ProductKey = StlProductKeys.MaintainArr;

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
                "Handoff code is not intended for MaintainArr.",
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
            MaintainArrSuiteLaunchCatalog.OrdinaryProductKeys,
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
            MaintainArrSuiteLaunchCatalog.OrdinaryProductKeys,
            redeemed.ThemePreference,
            redeemed.CallbackUrl);
    }
}
