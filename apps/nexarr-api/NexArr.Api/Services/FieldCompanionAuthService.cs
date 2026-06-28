using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionAuthService(
    NexArrDbContext db,
    ITokenService tokenService,
    IPlatformAuditService audit,
    FieldCompanionNotificationEnqueueService notificationEnqueueService,
    PlatformSessionSettingsService sessionSettingsService)
{
    private const string ProductKey = "fieldcompanion";

    public async Task<FieldCompanionSessionResponse> RedeemHandoffAsync(
        FieldCompanionRedeemHandoffRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.HandoffCode))
        {
            throw new StlApiException("fieldcompanion.handoff_missing", "Handoff code is required.", 400);
        }

        var record = await db.HandoffCodes
            .Include(h => h.User)
            .Include(h => h.Tenant)
            .FirstOrDefaultAsync(
                h => h.CodeHash == LaunchService.HashHandoffCode(request.HandoffCode.Trim()),
                cancellationToken)
            ?? throw new StlApiException("fieldcompanion.handoff_invalid", "Handoff code is invalid or expired.", 401);

        if (!string.Equals(record.TargetProductKey, ProductKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "fieldcompanion.handoff_product_mismatch",
                "Handoff code is not intended for the fieldcompanion app.",
                403);
        }

        if (record.RedeemedAt is not null)
        {
            throw new StlApiException("fieldcompanion.handoff_already_redeemed", "Handoff code has already been redeemed.", 409);
        }

        if (record.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new StlApiException("fieldcompanion.handoff_expired", "Handoff code has expired.", 401);
        }

        if (record.Tenant.Status != TenantStatuses.Active || !record.User.IsActive)
        {
            throw new StlApiException("fieldcompanion.launch_denied", "Tenant or user is not active.", 403);
        }

        var hasActiveMembership = await db.TenantMemberships.AsNoTracking().AnyAsync(
            membership => membership.TenantId == record.TenantId
                && membership.UserId == record.UserId
                && membership.IsActive,
            cancellationToken);

        if (!hasActiveMembership)
        {
            throw new StlApiException(
                "fieldcompanion.membership_inactive",
                "Tenant membership is no longer active.",
                403);
        }

        var requestedByPersonId = record.RequestedByPersonId
            ?? throw new StlApiException(
                "fieldcompanion.handoff_missing_person",
                "Handoff code is missing requested person identity.",
                500);
        record.RedeemedAt = DateTimeOffset.UtcNow;

        var launchableProductKeys = FieldCompanionSuiteLaunchCatalog.OrdinaryProductKeys;

        var membershipRoleKey = await db.TenantMemberships
            .AsNoTracking()
            .Where(m => m.TenantId == record.TenantId && m.UserId == record.UserId && m.IsActive)
            .Select(m => m.RoleKey)
            .FirstOrDefaultAsync(cancellationToken);

        var tenantRoleKey = !string.IsNullOrWhiteSpace(membershipRoleKey)
            ? membershipRoleKey
            : (record.User.IsPlatformAdmin ? "platform_admin" : "tenant_member");

        var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
        var sessionId = Guid.NewGuid();
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(settings.RefreshTokenDays);

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
            launchableProductKeys,
            tenantRoleKey,
            requestedByPersonId,
            settings.AccessTokenMinutes);

        await audit.WriteAsync(
            "fieldcompanion.handoff.redeem",
            "handoff_code",
            record.Id.ToString(),
            "Success",
            tenantId: record.TenantId,
            actorUserId: record.UserId,
            cancellationToken: cancellationToken);

        await notificationEnqueueService.TryEnqueueAsync(
            record.TenantId,
            FieldCompanionNotificationEventKinds.HandoffRedeemed,
            record.UserId,
            "handoff_code",
            record.Id,
            cancellationToken);

        return new FieldCompanionSessionResponse(
            accessToken,
            refreshToken,
            accessExpires,
            refreshExpires,
            sessionId,
            record.UserId,
            requestedByPersonId,
            record.User.Email,
            record.User.DisplayName,
            record.TenantId,
            record.Tenant.Slug,
            record.Tenant.DisplayName,
            tenantRoleKey,
            record.User.IsPlatformAdmin,
            launchableProductKeys,
            string.IsNullOrWhiteSpace(record.User.ThemePreference) ? "dark" : record.User.ThemePreference,
            record.CallbackUrl);
    }

    public FieldCompanionMeResponse GetMe(ClaimsPrincipal principal)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        return new FieldCompanionMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
            principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value ?? string.Empty,
            principal.GetTenantId(),
            string.Empty,
            string.Empty,
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            FieldCompanionSuiteLaunchCatalog.OrdinaryProductKeys,
            FieldInboxRules.FieldProductKeys.ToList());
    }
}

