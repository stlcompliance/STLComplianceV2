using LoadArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace LoadArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    LoadArrTokenService tokenService)
{
    private const string ProductKey = StlProductKeys.LoadArr;

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
                "Handoff code is not intended for LoadArr.",
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
            LoadArrSuiteLaunchCatalog.OrdinaryProductKeys,
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
            LoadArrSuiteLaunchCatalog.OrdinaryProductKeys,
            redeemed.ThemePreference,
            redeemed.CallbackUrl);
    }
}
