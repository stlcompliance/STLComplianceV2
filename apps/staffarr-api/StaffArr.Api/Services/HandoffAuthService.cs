using StaffArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class HandoffAuthService(
    NexArrHandoffClient nexArrHandoff,
    StaffArrTokenService tokenService)
{
    private const string ProductKey = "staffarr";

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
                "Handoff code is not intended for StaffArr.",
                403);
        }

        var entitled = redeemed.Entitlements.Contains(ProductKey, StringComparer.OrdinalIgnoreCase);
        if (!entitled)
        {
            throw new StlApiException(
                "handoff.not_entitled",
                "Tenant does not have an active StaffArr entitlement.",
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
            redeemed.Entitlements,
            isPlatformAdmin: false);

        return new HandoffSessionResponse(
            accessToken,
            expiresAt,
            redeemed.UserId,
            personId,
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId,
            redeemed.TenantSlug,
            redeemed.SessionId,
            redeemed.Entitlements);
    }
}
