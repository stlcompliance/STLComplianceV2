using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class AuthService(
    NexArrDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue,
    IOptions<StlJwtOptions> jwtOptions)
{
    public const int FailedLoginLockoutThreshold = 5;
    public const int LockoutMinutes = 15;

    public async Task<AuthTokenResponse> LoginAsync(
        LoginRequest request,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.Credential)
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || user.Credential is null || !user.IsActive)
        {
            await audit.WriteAsync("auth.login", "user", email, "Denied", reasonCode: "invalid_credentials", cancellationToken: cancellationToken);
            throw new StlApiException("auth.invalid_credentials", "Invalid email or password.", 401);
        }

        var now = DateTimeOffset.UtcNow;
        var wasLocked = user.Credential.LockedUntil is DateTimeOffset lockedUntil && lockedUntil > now;
        if (wasLocked)
        {
            await audit.WriteAsync(
                "auth.login",
                "user",
                user.Id.ToString(),
                "Denied",
                actorUserId: user.Id,
                reasonCode: "account_locked",
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.account_locked", "Account is temporarily locked.", 423);
        }

        if (!passwordHasher.Verify(request.Password, user.Credential.PasswordHash))
        {
            await RecordFailedLoginAsync(user, now, cancellationToken);
            throw new StlApiException("auth.invalid_credentials", "Invalid email or password.", 401);
        }

        if (!user.Credential.IsEmailVerified)
        {
            await audit.WriteAsync(
                "auth.login",
                "user",
                user.Id.ToString(),
                "Denied",
                actorUserId: user.Id,
                reasonCode: "email_not_verified",
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.email_not_verified", "Email verification is required before sign in.", 403);
        }

        if (ShouldRequirePlatformAdminMfa(user) && !user.Credential.IsMfaEnabled)
        {
            await audit.WriteAsync(
                "auth.login",
                "user",
                user.Id.ToString(),
                "Denied",
                actorUserId: user.Id,
                reasonCode: "mfa_required",
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.mfa_required", "Multi-factor authentication is required before sign in.", 403);
        }

        var shouldEmitUnlock = user.Credential.LockedUntil is DateTimeOffset expiredLock && expiredLock <= now;
        if (user.Credential.FailedLoginCount != 0 || user.Credential.LockedUntil is not null)
        {
            user.Credential.FailedLoginCount = 0;
            user.Credential.LockedUntil = null;
            user.ModifiedAt = now;
        }

        var tenantId = await ResolveTenantIdAsync(user, request.TenantId, cancellationToken);
        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant.Status != TenantStatuses.Active)
        {
            await audit.WriteAsync("auth.login", "tenant", tenant.Id.ToString(), "Denied", tenantId: tenant.Id, actorUserId: user.Id, reasonCode: "tenant_suspended", cancellationToken: cancellationToken);
            throw new StlApiException("auth.tenant_suspended", "Tenant is not active.", 403);
        }

        var entitlements = await GetActiveEntitlementsAsync(tenantId, cancellationToken);
        if (entitlements.Count == 0 && !user.IsPlatformAdmin)
        {
            await audit.WriteAsync("auth.login", "tenant", tenant.Id.ToString(), "Denied", tenantId: tenant.Id, actorUserId: user.Id, reasonCode: "no_entitlements", cancellationToken: cancellationToken);
            throw new StlApiException("auth.no_entitlements", "No active product entitlements for this tenant.", 403);
        }

        if (await IsSuspiciousLoginAsync(user.Id, userAgent, ipAddress, cancellationToken))
        {
            await audit.WriteAsync(
                "auth.suspicious_login",
                "user",
                user.Id.ToString(),
                "Warning",
                tenantId: tenantId,
                actorUserId: user.Id,
                reasonCode: "new_device_or_ip",
                cancellationToken: cancellationToken);
        }

        var response = await IssueSessionAsync(user, tenantId, entitlements, userAgent, ipAddress, request.RememberDevice, cancellationToken);

        if (shouldEmitUnlock)
        {
            await EnqueueUserLifecycleEventAsync(
                PlatformOutboxEventKinds.UserUnlocked,
                user,
                "Platform user automatically unlocked after successful login.",
                new Dictionary<string, string>
                {
                    ["source"] = "login_lockout",
                },
                cancellationToken);
        }

        return response;
    }

    public async Task<AuthTokenResponse> RenewAsync(
        RenewSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var session = await db.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, cancellationToken);

        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new StlApiException("auth.invalid_refresh_token", "Refresh token is invalid or expired.", 401);
        }

        if (!session.User.IsActive)
        {
            throw new StlApiException("auth.user_inactive", "User account is inactive.", 403);
        }

        var tenantId = session.ActiveTenantId
            ?? throw new StlApiException("auth.session_invalid", "Session has no active tenant.", 401);

        var entitlements = await GetActiveEntitlementsAsync(tenantId, cancellationToken);
        session.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "auth.renew",
            "session",
            session.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: session.UserId,
            cancellationToken: cancellationToken);

        return await IssueSessionAsync(session.User, tenantId, entitlements, session.UserAgent, session.IpAddress, session.IsRemembered, cancellationToken);
    }

    private bool ShouldRequirePlatformAdminMfa(PlatformUser user)
    {
        if (!user.IsPlatformAdmin)
        {
            return false;
        }

        var configuredValue =
            Environment.GetEnvironmentVariable("AUTH_REQUIRE_PLATFORM_ADMIN_MFA")
            ?? Environment.GetEnvironmentVariable("Auth__RequirePlatformAdminMfa")
            ?? jwtOptions.Value.RequirePlatformAdminMfa?.ToString()
            ?? string.Empty;

        return bool.TryParse(configuredValue, out var requireMfa) && requireMfa;
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var session = await db.UserSessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, cancellationToken);
        if (session is null)
        {
            return;
        }

        session.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "auth.logout",
            "session",
            session.Id.ToString(),
            "Success",
            tenantId: session.ActiveTenantId,
            actorUserId: session.UserId,
            cancellationToken: cancellationToken);
    }

    public async Task<MeResponse> GetMeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var context = ParsePrincipal(principal);
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == context.UserId, cancellationToken);
        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == context.TenantId, cancellationToken);
        var entitlements = await GetActiveEntitlementsAsync(context.TenantId, cancellationToken);

        return new MeResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsPlatformAdmin,
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            entitlements);
    }

    public async Task<IReadOnlyList<TenantSummary>> GetMyTenantsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from m in db.TenantMemberships.AsNoTracking()
            where m.UserId == userId && m.IsActive
            join t in db.Tenants.AsNoTracking() on m.TenantId equals t.Id
            orderby t.DisplayName
            select new TenantSummary(t.Id, t.Slug, t.DisplayName, t.Status, m.RoleKey))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EntitlementSummary>> GetMyEntitlementsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from e in db.Entitlements.AsNoTracking()
            where e.TenantId == tenantId && e.Status == EntitlementStatuses.Active
            join p in db.ProductCatalog.AsNoTracking() on e.ProductKey equals p.ProductKey
            orderby p.DisplayName
            select new EntitlementSummary(p.ProductKey, p.DisplayName, e.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSessionsResponse> GetMySessionsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userId = principal.GetUserId();
        var currentSessionId = principal.GetSessionId();
        var now = DateTimeOffset.UtcNow;

        var sessions = await db.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new UserSessionSummary(
                s.Id,
                s.CreatedAt,
                s.ExpiresAt,
                s.RevokedAt,
                s.UserAgent,
                s.IpAddress,
                s.ActiveTenantId,
                s.Id == currentSessionId,
                s.RevokedAt == null && s.ExpiresAt > now,
                s.IsRemembered))
            .ToListAsync(cancellationToken);

        return new UserSessionsResponse(sessions);
    }

    public async Task RevokeMySessionAsync(
        ClaimsPrincipal principal,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var userId = principal.GetUserId();
        var session = await db.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session is null)
        {
            throw new StlApiException("auth.session_not_found", "Session was not found.", 404);
        }

        if (session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "auth.session_revoked",
            "session",
            session.Id.ToString(),
            "Success",
            tenantId: session.ActiveTenantId,
            actorUserId: userId,
            cancellationToken: cancellationToken);
    }

    public async Task<NavigationResponse> GetNavigationAsync(
        ClaimsPrincipal principal,
        string? currentProductKey = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var entitlements = principal.GetEntitlements();
        var isPlatformAdmin = principal.IsPlatformAdmin();
        var normalizedCurrentProductKey = currentProductKey?.Trim().ToLowerInvariant();

        var catalogProducts = await db.Entitlements
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == EntitlementStatuses.Active)
            .Join(db.ProductCatalog.AsNoTracking().Where(p => p.IsActive), e => e.ProductKey, p => p.ProductKey, (e, p) => p)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        var products = catalogProducts
            .Select(p =>
            {
                var entitled = entitlements.Contains(p.ProductKey, StringComparer.OrdinalIgnoreCase);
                var surfaces = ProductSurfaceCatalog.BuildSurfaces(p.ProductKey, entitled, isPlatformAdmin);
                return new NavigationItem(
                    p.ProductKey,
                    p.DisplayName,
                    p.ProductCategory,
                    p.ProductStatus,
                    $"/app/{p.ProductKey}",
                    $"/app/{p.ProductKey}/launch",
                    string.Equals(p.ProductKey, normalizedCurrentProductKey, StringComparison.OrdinalIgnoreCase),
                    p.SortOrder,
                    surfaces);
            })
            .ToList();

        return new NavigationResponse(tenantId, products);
    }

    private async Task<AuthTokenResponse> IssueSessionAsync(
        PlatformUser user,
        Guid tenantId,
        IReadOnlyList<string> entitlements,
        string? userAgent,
        string? ipAddress,
        bool rememberDevice,
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid();
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshLifetimeDays = ResolveRefreshTokenLifetimeDays(rememberDevice);
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(refreshLifetimeDays);

        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserId = user.Id,
            RefreshTokenHash = tokenService.HashRefreshToken(refreshToken),
            ActiveTenantId = tenantId,
            IsRemembered = rememberDevice,
            ExpiresAt = refreshExpires,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpires) = tokenService.CreateAccessToken(user, tenantId, sessionId, entitlements);

        await audit.WriteAsync(
            "auth.login",
            "session",
            sessionId.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: user.Id,
            cancellationToken: cancellationToken);

        return new AuthTokenResponse(
            accessToken,
            refreshToken,
            accessExpires,
            refreshExpires,
            sessionId,
            user.Id,
            tenantId);
    }

    private int ResolveRefreshTokenLifetimeDays(bool rememberDevice)
    {
        var configuredDefault = jwtOptions.Value.RefreshTokenDays > 0
            ? jwtOptions.Value.RefreshTokenDays
            : 7;

        if (!rememberDevice)
        {
            return configuredDefault;
        }

        var configuredRemembered = jwtOptions.Value.RememberedRefreshTokenDays;
        if (configuredRemembered is null || configuredRemembered <= 0)
        {
            return configuredDefault;
        }

        return configuredRemembered.Value;
    }

    private async Task RecordFailedLoginAsync(
        PlatformUser user,
        DateTimeOffset failedAt,
        CancellationToken cancellationToken)
    {
        user.Credential!.FailedLoginCount += 1;
        user.ModifiedAt = failedAt;

        var lockTriggered = false;
        if (user.Credential.FailedLoginCount >= FailedLoginLockoutThreshold)
        {
            user.Credential.LockedUntil = failedAt.AddMinutes(LockoutMinutes);
            lockTriggered = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "auth.login",
            "user",
            user.Id.ToString(),
            "Denied",
            actorUserId: user.Id,
            reasonCode: lockTriggered ? "account_locked" : "invalid_credentials",
            cancellationToken: cancellationToken);

        if (lockTriggered)
        {
            await audit.WriteAsync(
                "user.locked",
                "user",
                user.Id.ToString(),
                "Success",
                actorUserId: user.Id,
                reasonCode: "failed_login_threshold",
                cancellationToken: cancellationToken);

            await EnqueueUserLifecycleEventAsync(
                PlatformOutboxEventKinds.UserLocked,
                user,
                "Platform user automatically locked after repeated failed logins.",
                new Dictionary<string, string>
                {
                    ["source"] = "login_lockout",
                    ["reason"] = "failed_login_threshold",
                },
                cancellationToken);
        }
    }

    private async Task<bool> IsSuspiciousLoginAsync(
        Guid userId,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var latestPriorSession = await db.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (latestPriorSession is null)
        {
            return false;
        }

        var normalizedUserAgent = NormalizeSuspicionValue(userAgent);
        var normalizedIpAddress = NormalizeSuspicionValue(ipAddress);
        var previousUserAgent = NormalizeSuspicionValue(latestPriorSession.UserAgent);
        var previousIpAddress = NormalizeSuspicionValue(latestPriorSession.IpAddress);

        var userAgentChanged =
            !string.IsNullOrEmpty(normalizedUserAgent)
            && !string.IsNullOrEmpty(previousUserAgent)
            && !string.Equals(normalizedUserAgent, previousUserAgent, StringComparison.Ordinal);
        var ipChanged =
            !string.IsNullOrEmpty(normalizedIpAddress)
            && !string.IsNullOrEmpty(previousIpAddress)
            && !string.Equals(normalizedIpAddress, previousIpAddress, StringComparison.Ordinal);

        return userAgentChanged || ipChanged;
    }

    private static string NormalizeSuspicionValue(string? value) =>
        value?.Trim().ToLowerInvariant() ?? string.Empty;

    private async Task EnqueueUserLifecycleEventAsync(
        string eventType,
        PlatformUser user,
        string summary,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        metadata["email"] = user.Email;
        metadata["failedLoginCount"] = user.Credential?.FailedLoginCount.ToString() ?? "0";
        metadata["lockedUntil"] = user.Credential?.LockedUntil?.ToString("O") ?? string.Empty;

        await outboxEnqueue.TryEnqueueAsync(
            eventType,
            "user",
            user.Id.ToString(),
            user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                null,
                user.Id,
                "user",
                user.Id.ToString(),
                summary,
                metadata),
            cancellationToken: cancellationToken);
    }

    private async Task<Guid> ResolveTenantIdAsync(
        PlatformUser user,
        Guid? requestedTenantId,
        CancellationToken cancellationToken)
    {
        var memberships = user.Memberships.Where(m => m.IsActive).ToList();
        if (memberships.Count == 0 && !user.IsPlatformAdmin)
        {
            throw new StlApiException("auth.no_tenant_membership", "User has no active tenant memberships.", 403);
        }

        if (requestedTenantId is Guid tenantId)
        {
            if (!user.IsPlatformAdmin && memberships.All(m => m.TenantId != tenantId))
            {
                throw new StlApiException("auth.tenant_forbidden", "User is not a member of the requested tenant.", 403);
            }

            return tenantId;
        }

        return memberships.FirstOrDefault()?.TenantId
            ?? await db.Tenants.OrderBy(t => t.CreatedAt).Select(t => t.Id).FirstAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetActiveEntitlementsAsync(
        Guid tenantId,
        CancellationToken cancellationToken) =>
        await db.Entitlements
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == EntitlementStatuses.Active)
            .Select(e => e.ProductKey)
            .ToListAsync(cancellationToken);

    private static (Guid UserId, Guid TenantId) ParsePrincipal(ClaimsPrincipal principal)
    {
        try
        {
            return (principal.GetUserId(), principal.GetTenantId());
        }
        catch (InvalidOperationException)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }
}
