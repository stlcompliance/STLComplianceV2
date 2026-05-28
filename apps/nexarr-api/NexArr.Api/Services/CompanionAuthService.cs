using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionAuthService(
    NexArrDbContext db,
    ITokenService tokenService,
    IPlatformAuditService audit,
    IOptions<StlJwtOptions> jwtOptions)
{
    private const string ProductKey = "companion";

    public async Task<CompanionSessionResponse> RedeemHandoffAsync(
        CompanionRedeemHandoffRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.HandoffCode))
        {
            throw new StlApiException("companion.handoff_missing", "Handoff code is required.", 400);
        }

        var record = await db.HandoffCodes
            .Include(h => h.User)
            .Include(h => h.Tenant)
            .FirstOrDefaultAsync(
                h => h.CodeHash == LaunchService.HashHandoffCode(request.HandoffCode.Trim()),
                cancellationToken)
            ?? throw new StlApiException("companion.handoff_invalid", "Handoff code is invalid or expired.", 401);

        if (!string.Equals(record.TargetProductKey, ProductKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "companion.handoff_product_mismatch",
                "Handoff code is not intended for the Companion app.",
                403);
        }

        if (record.RedeemedAt is not null)
        {
            throw new StlApiException("companion.handoff_already_redeemed", "Handoff code has already been redeemed.", 409);
        }

        if (record.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new StlApiException("companion.handoff_expired", "Handoff code has expired.", 401);
        }

        if (record.Tenant.Status != TenantStatuses.Active || !record.User.IsActive)
        {
            throw new StlApiException("companion.launch_denied", "Tenant or user is not active.", 403);
        }

        var entitled = await db.Entitlements.AnyAsync(
            e => e.TenantId == record.TenantId
                && e.ProductKey == ProductKey
                && e.Status == EntitlementStatuses.Active,
            cancellationToken);

        if (!entitled && !record.User.IsPlatformAdmin)
        {
            throw new StlApiException(
                "companion.not_entitled",
                "Tenant does not have an active Companion app entitlement.",
                403);
        }

        record.RedeemedAt = DateTimeOffset.UtcNow;

        var entitlements = await db.Entitlements
            .AsNoTracking()
            .Where(e => e.TenantId == record.TenantId && e.Status == EntitlementStatuses.Active)
            .Select(e => e.ProductKey)
            .ToListAsync(cancellationToken);

        var membershipRoleKey = await db.TenantMemberships
            .AsNoTracking()
            .Where(m => m.TenantId == record.TenantId && m.UserId == record.UserId && m.IsActive)
            .Select(m => m.RoleKey)
            .FirstOrDefaultAsync(cancellationToken);

        var tenantRoleKey = !string.IsNullOrWhiteSpace(membershipRoleKey)
            ? membershipRoleKey
            : (record.User.IsPlatformAdmin ? "platform_admin" : "tenant_member");

        var sessionId = Guid.NewGuid();
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays);

        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserId = record.UserId,
            RefreshTokenHash = tokenService.HashRefreshToken(refreshToken),
            ActiveTenantId = record.TenantId,
            ExpiresAt = refreshExpires,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpires) = tokenService.CreateSessionAccessToken(
            record.User,
            record.TenantId,
            sessionId,
            entitlements,
            tenantRoleKey,
            record.UserId);

        await audit.WriteAsync(
            "companion.handoff.redeem",
            "handoff_code",
            record.Id.ToString(),
            "Success",
            tenantId: record.TenantId,
            actorUserId: record.UserId,
            cancellationToken: cancellationToken);

        return new CompanionSessionResponse(
            accessToken,
            refreshToken,
            accessExpires,
            refreshExpires,
            sessionId,
            record.UserId,
            record.UserId,
            record.User.Email,
            record.User.DisplayName,
            record.TenantId,
            record.Tenant.Slug,
            record.Tenant.DisplayName,
            tenantRoleKey,
            record.User.IsPlatformAdmin,
            entitlements);
    }

    public CompanionMeResponse GetMe(ClaimsPrincipal principal)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        var entitlements = principal.GetEntitlements();
        return new CompanionMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
            principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value ?? string.Empty,
            principal.GetTenantId(),
            string.Empty,
            string.Empty,
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            entitlements,
            FieldInboxRules.FieldProductKeys
                .Where(productKey => principal.IsPlatformAdmin() || entitlements.Contains(productKey, StringComparer.OrdinalIgnoreCase))
                .ToList());
    }
}
