using ReportArr.Api.Contracts;
using ReportArr.Api.Data;
using ReportArr.Api.Models;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace ReportArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    ReportArrTokenService tokenService)
{
    private const string ProductKey = "reportarr";

    public async Task<ReportArrHandoffSessionResponse> RedeemAsync(
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
                "Handoff code is not intended for ReportArr.",
                403);
        }

        if (!redeemed.Entitlements.Contains(ProductKey, StringComparer.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "handoff.not_entitled",
                "Tenant does not have an active ReportArr entitlement.",
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
            redeemed.IsPlatformAdmin,
            redeemed.AccessTokenMinutes);

        return new ReportArrHandoffSessionResponse(
            accessToken,
            expiresAt.ToString("O"),
            redeemed.UserId.ToString(),
            redeemed.UserId.ToString(),
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId.ToString(),
            redeemed.TenantSlug,
            redeemed.TenantDisplayName,
            redeemed.SessionId.ToString(),
            redeemed.TenantRoleKey,
            redeemed.IsPlatformAdmin,
            redeemed.Entitlements,
            redeemed.ThemePreference,
            redeemed.CallbackUrl);
    }
}
