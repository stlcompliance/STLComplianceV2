using StaffArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class HandoffAuthService(
    NexArrHandoffClient nexArrHandoff,
    StaffArrTokenService tokenService,
    PersonProvisioningService personProvisioning)
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

        var person = await personProvisioning.EnsurePersonAsync(
            redeemed.TenantId,
            redeemed.UserId,
            redeemed.Email,
            redeemed.DisplayName,
            cancellationToken);
        var (accessToken, expiresAt) = tokenService.CreateAccessToken(
            redeemed.UserId,
            person.Id,
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
            person.Id,
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
