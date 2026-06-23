using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformIdentitySecurityService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public const string RequestPasswordResetActionScope = "nexarr.users.password_reset";
    public const string ResetMfaActionScope = "nexarr.users.mfa_reset";

    public static readonly Guid IntegrationActorUserId = Guid.Parse("00000000-0000-0000-0000-00000000000d");

    public async Task<RequestPlatformIdentityPasswordResetResponse> RequestPasswordResetAsync(
        RequestPlatformIdentityPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = request.RequestedByUserId ?? IntegrationActorUserId;
        var reason = NormalizeReason(request.Reason, "StaffArr password reset");

        var user = await LoadEligibleUserAsync(request.TenantId, request.ExternalUserId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var plaintextToken = PasswordResetService.GenerateResetToken();
        var tokenHash = PasswordResetService.HashResetToken(plaintextToken);

        var pendingTokens = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);
        foreach (var pending in pendingTokens)
        {
            pending.UsedAt = now;
        }

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddMinutes(PasswordResetRules.TokenLifetimeMinutes),
            CreatedAt = now,
        });

        user.ModifiedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "auth.password_reset_requested",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            reasonCode: reason,
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.UserUpdated,
            "user",
            user.Id.ToString(),
            user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                request.TenantId,
                actorUserId,
                "user",
                user.Id.ToString(),
                "Password reset requested from StaffArr.",
                new Dictionary<string, string>
                {
                    ["staffarrPersonId"] = request.StaffarrPersonId.ToString(),
                    ["reason"] = reason,
                    ["requestedByUserId"] = actorUserId.ToString(),
                }),
            cancellationToken: cancellationToken);

        return new RequestPlatformIdentityPasswordResetResponse(
            request.ExternalUserId,
            "Password reset instructions were sent if the account can receive them.");
    }

    public async Task<ResetPlatformIdentityMfaResponse> ResetMfaAsync(
        ResetPlatformIdentityMfaRequest request,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = request.RequestedByUserId ?? IntegrationActorUserId;
        var reason = NormalizeReason(request.Reason, "StaffArr MFA reset");

        var user = await LoadEligibleUserAsync(request.TenantId, request.ExternalUserId, cancellationToken);
        if (user.Credential is null)
        {
            throw new StlApiException("user.login_not_enabled", "Platform user does not have login credentials.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var wasMfaEnabled = user.Credential.IsMfaEnabled || !string.IsNullOrWhiteSpace(user.Credential.MfaSecret);

        user.Credential.IsMfaEnabled = false;
        user.Credential.MfaSecret = null;
        user.Credential.MfaRecoveryCodeHashesJson = null;
        user.ModifiedAt = now;

        var sessions = await db.UserSessions
            .Where(s => s.UserId == user.Id && s.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.mfa_disabled",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            reasonCode: reason,
            cancellationToken: cancellationToken);

        foreach (var session in sessions)
        {
            await audit.WriteAsync(
                "auth.session_revoked",
                "session",
                session.Id.ToString(),
                "Success",
                tenantId: session.ActiveTenantId,
                actorUserId: actorUserId,
                reasonCode: "mfa_reset",
                cancellationToken: cancellationToken);
        }

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.UserUpdated,
            "user",
            user.Id.ToString(),
            user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                request.TenantId,
                actorUserId,
                "user",
                user.Id.ToString(),
                "MFA reset from StaffArr.",
                new Dictionary<string, string>
                {
                    ["staffarrPersonId"] = request.StaffarrPersonId.ToString(),
                    ["reason"] = reason,
                    ["requestedByUserId"] = actorUserId.ToString(),
                    ["sessionsRevoked"] = sessions.Count.ToString(),
                }),
            cancellationToken: cancellationToken);

        return new ResetPlatformIdentityMfaResponse(
            request.ExternalUserId,
            wasMfaEnabled,
            user.ModifiedAt);
    }

    private async Task<PlatformUser> LoadEligibleUserAsync(
        Guid tenantId,
        Guid externalUserId,
        CancellationToken cancellationToken)
    {
        var tenantExists = await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == externalUserId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var hasMembership = await db.TenantMemberships.AnyAsync(
            m => m.TenantId == tenantId
                && m.UserId == externalUserId
                && m.IsActive,
            cancellationToken);
        if (!hasMembership)
        {
            throw new StlApiException(
                "user.tenant_membership_missing",
                "User does not have an active membership for this tenant.",
                403);
        }

        return user;
    }

    private static string NormalizeReason(string? reason, string fallback)
    {
        var trimmed = reason?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return fallback;
        }

        return trimmed.Length <= 512 ? trimmed : trimmed[..512];
    }
}
