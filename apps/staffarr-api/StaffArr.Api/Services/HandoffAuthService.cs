using StaffArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace StaffArr.Api.Services;

public sealed class HandoffAuthService(
    StlNexArrHandoffClient nexArrHandoff,
    StaffArrTokenService tokenService,
    PersonProvisioningService personProvisioning)
{
    private const string ProductKey = StlProductKeys.StaffArr;

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
            StaffArrSuiteLaunchCatalog.OrdinaryProductKeys,
            redeemed.IsPlatformAdmin,
            redeemed.AccessTokenMinutes);

        return new HandoffSessionResponse(
            accessToken,
            expiresAt,
            redeemed.UserId,
            person.Id,
            redeemed.Email,
            redeemed.DisplayName,
            redeemed.TenantId,
            redeemed.TenantSlug,
            redeemed.TenantDisplayName,
            redeemed.SessionId,
            redeemed.TenantRoleKey,
            redeemed.IsPlatformAdmin,
            StaffArrSuiteLaunchCatalog.OrdinaryProductKeys,
            redeemed.ThemePreference,
            redeemed.CallbackUrl);
    }
}
