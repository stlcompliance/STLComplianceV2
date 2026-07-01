using System.Security.Claims;
using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
    PlatformSessionSettingsService sessionSettingsService,
    MfaService mfaService,
    MfaSecretProtector mfaSecretProtector,
    PlatformAuthorizationService authorization,
    FixedSuiteProductAccessService productAccess,
    LocalDevAuthBypassPolicy localDevAuthBypassPolicy)
{
    public const int FailedLoginLockoutThreshold = 5;
    public const int LockoutMinutes = 15;
    private const string LaunchableProductStatus = "launchable";

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

        var requiresMfa = await ShouldRequireMfaAsync(user, cancellationToken);
        if (requiresMfa && !await VerifyMfaAsync(user, request, now, cancellationToken))
        {
            await audit.WriteAsync(
                "auth.login",
                "user",
                user.Id.ToString(),
                "Denied",
                actorUserId: user.Id,
                reasonCode: request.MfaCode is null && request.RecoveryCode is null ? "mfa_required" : "invalid_mfa",
                cancellationToken: cancellationToken);
            throw new StlApiException(
                request.MfaCode is null && request.RecoveryCode is null
                    ? "auth.mfa_required"
                    : "auth.invalid_mfa_code",
                request.MfaCode is null && request.RecoveryCode is null
                    ? "Multi-factor authentication is required before sign in."
                    : "Invalid multi-factor authentication code.",
                403);
        }

        var shouldEmitUnlock = user.Credential.LockedUntil is DateTimeOffset expiredLock && expiredLock <= now;
        if (user.Credential.FailedLoginCount != 0 || user.Credential.LockedUntil is not null)
        {
            user.Credential.FailedLoginCount = 0;
            user.Credential.LockedUntil = null;
            user.ModifiedAt = now;
        }

        var tenant = await ResolveTenantAsync(user, request.TenantId, cancellationToken);
        var tenantId = tenant.Id;

        if (tenant.Status != TenantStatuses.Active)
        {
            await audit.WriteAsync("auth.login", "tenant", tenant.Id.ToString(), "Denied", tenantId: tenant.Id, actorUserId: user.Id, reasonCode: "tenant_suspended", cancellationToken: cancellationToken);
            throw new StlApiException("auth.tenant_suspended", "Tenant is not active.", 403);
        }

        var launchableProductKeys = await GetAccessibleProductsAsync(user.IsPlatformAdmin, cancellationToken);

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

        var response = await IssueSessionAsync(user, tenantId, launchableProductKeys, userAgent, ipAddress, request.RememberDevice, cancellationToken);

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

    public async Task<AuthTokenResponse> LocalDevBypassLoginAsync(
        LocalDevBypassLoginRequest request,
        HttpRequest httpRequest,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (!localDevAuthBypassPolicy.TryAuthorize(httpRequest, out var authorizationReason))
        {
            await audit.WriteAsync(
                "local-dev-auth",
                "user",
                request.Email.Trim().ToLowerInvariant(),
                "Denied",
                reasonCode: authorizationReason,
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.local_dev_bypass_forbidden", "Local development auth bypass is not available.", 403);
        }

        return await IssueLocalDevBypassSessionAsync(
            request.Email,
            request.TenantId,
            request.RememberDevice,
            userAgent,
            ipAddress,
            cancellationToken);
    }

    public async Task<AuthTokenResponse?> TryAutoLocalDevBypassSessionAsync(
        HttpRequest httpRequest,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (!localDevAuthBypassPolicy.TryAuthorize(httpRequest, out _))
        {
            return null;
        }

        return await IssueLocalDevBypassSessionAsync(
            localDevAuthBypassPolicy.ResolveDefaultEmail(),
            null,
            rememberDevice: true,
            userAgent,
            ipAddress,
            cancellationToken);
    }

    public async Task<AuthTokenResponse> RenewAsync(
        RenewSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var now = DateTimeOffset.UtcNow;
        var session = await db.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, cancellationToken);

        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            throw new StlApiException("auth.invalid_refresh_token", "Refresh token is invalid or expired.", 401);
        }

        if (!session.User.IsActive)
        {
            throw new StlApiException("auth.user_inactive", "User account is inactive.", 403);
        }

        var tenantId = session.ActiveTenantId
            ?? throw new StlApiException("auth.session_invalid", "Session has no active tenant.", 401);

        await RequireActiveTenantMembershipAsync(session.UserId, tenantId, cancellationToken);
        var launchableProductKeys = await GetAccessibleProductsAsync(session.User.IsPlatformAdmin, cancellationToken);

        if (!db.Database.IsRelational())
        {
            var trackedSession = await db.UserSessions
                .Include(s => s.User)
                .FirstAsync(s => s.RefreshTokenHash == hash, cancellationToken);

            trackedSession.RevokedAt = now;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "auth.renew",
                "session",
                trackedSession.Id.ToString(),
                "Success",
                tenantId: tenantId,
                actorUserId: trackedSession.UserId,
                cancellationToken: cancellationToken);

            return await IssueSessionAsync(
                trackedSession.User,
                tenantId,
                launchableProductKeys,
                trackedSession.UserAgent,
                trackedSession.IpAddress,
                trackedSession.IsRemembered,
                cancellationToken);
        }

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
            : null;

        var affected = await db.UserSessions
            .Where(s =>
                s.RefreshTokenHash == hash
                && s.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.RevokedAt, now),
                cancellationToken);

        if (affected != 1)
        {
            throw new StlApiException("auth.invalid_refresh_token", "Refresh token is invalid or expired.", 401);
        }

        await audit.WriteAsync(
            "auth.renew",
            "session",
            session.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: session.UserId,
            cancellationToken: cancellationToken);

        var renewed = await IssueSessionAsync(session.User, tenantId, launchableProductKeys, session.UserAgent, session.IpAddress, session.IsRemembered, cancellationToken);

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return renewed;
    }

    private async Task<bool> ShouldRequireMfaAsync(
        PlatformUser user,
        CancellationToken cancellationToken)
    {
        if (user.Credential?.IsMfaEnabled == true)
        {
            return true;
        }

        if (!user.IsPlatformAdmin)
        {
            return false;
        }

        var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
        return settings.RequirePlatformAdminMfa ?? false;
    }

    private async Task<bool> VerifyMfaAsync(
        PlatformUser user,
        LoginRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (user.Credential is null || !user.Credential.IsMfaEnabled)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.RecoveryCode))
        {
            return await VerifyRecoveryCodeAsync(user, request.RecoveryCode!, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(request.MfaCode))
        {
            return false;
        }

        if (!mfaSecretProtector.TryResolvePlaintext(user.Credential.MfaSecret, out var mfaSecret))
        {
            return false;
        }

        return mfaService.VerifyTotp(mfaSecret, request.MfaCode!, now);
    }

    private async Task<bool> VerifyRecoveryCodeAsync(
        PlatformUser user,
        string recoveryCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Credential?.MfaRecoveryCodeHashesJson))
        {
            return false;
        }

        IReadOnlyList<string>? hashes;
        try
        {
            hashes = JsonSerializer.Deserialize<IReadOnlyList<string>>(user.Credential.MfaRecoveryCodeHashesJson);
        }
        catch (JsonException)
        {
            return false;
        }

        if (hashes is not { Count: > 0 })
        {
            return false;
        }

        var targetHash = mfaService.HashRecoveryCode(recoveryCode);
        if (!hashes.Contains(targetHash, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var remaining = hashes
            .Where(hash => !string.Equals(hash, targetHash, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        user.Credential!.MfaRecoveryCodeHashesJson = remaining.Length > 0 ? JsonSerializer.Serialize(remaining) : null;
        user.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
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
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

        var context = ParsePrincipal(principal);
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == context.UserId, cancellationToken);
        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == context.TenantId, cancellationToken);
        var launchableProductKeys = await GetAccessibleProductsAsync(user.IsPlatformAdmin, cancellationToken);

        return new MeResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsPlatformAdmin,
            user.Credential?.RequiresPasswordChange ?? false,
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            NormalizeThemePreference(user.ThemePreference),
            launchableProductKeys);
    }

    public async Task<UpdateMyPasswordResponse> UpdateMyPasswordAsync(
        ClaimsPrincipal principal,
        UpdateMyPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        var sessionId = principal.GetSessionId();
        var user = await db.Users
            .Include(x => x.Credential)
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);

        if (user.Credential is null)
        {
            throw new StlApiException("auth.login_not_enabled", "Platform user does not have login credentials.", 409);
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.Credential.PasswordHash))
        {
            throw new StlApiException("auth.invalid_credentials", "Current password is incorrect.", 401);
        }

        var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
        if (!PasswordResetRules.MeetsPasswordPolicy(
                request.NewPassword,
                settings.PasswordMinLength,
                settings.RequirePasswordComplexity))
        {
            throw new StlApiException(
                "auth.password_policy",
                PasswordResetRules.PasswordPolicyMessage(
                    settings.PasswordMinLength,
                    settings.RequirePasswordComplexity),
                400);
        }

        var now = DateTimeOffset.UtcNow;
        user.Credential.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.Credential.PasswordChangedAt = now;
        user.Credential.RequiresPasswordChange = false;
        user.ModifiedAt = now;

        foreach (var session in user.Sessions.Where(s => s.Id != sessionId && s.RevokedAt is null))
        {
            session.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.password_changed",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: user.Id,
            cancellationToken: cancellationToken);

        return new UpdateMyPasswordResponse(now);
    }

    public async Task<UserPreferencesResponse> UpdateMyPreferencesAsync(
        ClaimsPrincipal principal,
        UpdateMyPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        var tenantId = principal.GetTenantId();
        var themePreference = NormalizeThemePreference(request.ThemePreference);
        var user = await db.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        if (!string.Equals(user.ThemePreference, themePreference, StringComparison.Ordinal))
        {
            user.ThemePreference = themePreference;
            user.ModifiedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "user.preferences.update",
                "user",
                user.Id.ToString(),
                "Success",
                tenantId: tenantId,
                actorUserId: user.Id,
                cancellationToken: cancellationToken);
        }

        return new UserPreferencesResponse(themePreference);
    }

    public async Task<IReadOnlyList<TenantSummary>> GetMyTenantsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        return await (
            from m in db.TenantMemberships.AsNoTracking()
            where m.UserId == userId && m.IsActive
            join t in db.Tenants.AsNoTracking() on m.TenantId equals t.Id
            orderby t.DisplayName
            select new TenantSummary(t.Id, t.Slug, t.DisplayName, t.Status, m.RoleKey))
            .ToListAsync(cancellationToken);
    }

    private static string NormalizeThemePreference(string? themePreference)
    {
        var normalized = (themePreference ?? "system").Trim().ToLowerInvariant();
        return normalized switch
        {
            "dark" => "dark",
            "light" => "light",
            "system" => "system",
            _ => throw new StlApiException(
                "preferences.theme_invalid",
                "Theme preference must be dark, light, or system.",
                400),
        };
    }

    public async Task<IReadOnlyList<LaunchableProductSummary>> GetMyLaunchableProductsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

        var products = await productAccess.ListAccessibleProductsAsync(
            principal.IsPlatformAdmin(),
            includeWorkers: false,
            cancellationToken);

        return products
            .OrderBy(product => product.DisplayName)
            .Select(product => new LaunchableProductSummary(product.ProductKey, product.DisplayName, LaunchableProductStatus))
            .ToList();
    }

    public async Task<UserSessionsResponse> GetMySessionsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

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
        await authorization.RequireActiveSessionAsync(principal, cancellationToken);

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
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        var tenantId = principal.GetTenantId();
        var isPlatformAdmin = principal.IsPlatformAdmin();
        var normalizedCurrentProductKey = string.IsNullOrWhiteSpace(currentProductKey)
            ? null
            : ProductKeyAliases.Normalize(currentProductKey);

        var catalogProducts = await productAccess.ListAccessibleProductsAsync(
            isPlatformAdmin,
            includeWorkers: true,
            cancellationToken);

        var products = catalogProducts
            .Select(p =>
            {
                var productAvailable = true;
                var surfaces = ProductSurfaceCatalog.BuildSurfaces(
                    p.ProductKey,
                    p.ProductStatus,
                    productAvailable,
                    isPlatformAdmin);
                var routePath = BuildNavigationRoutePath(p.ProductKey);
                return new NavigationItem(
                    p.ProductKey,
                    p.DisplayName,
                    p.ProductCategory,
                    p.ProductStatus,
                    routePath,
                    $"{routePath}/launch",
                    string.Equals(ProductKeyAliases.Normalize(p.ProductKey), normalizedCurrentProductKey, StringComparison.OrdinalIgnoreCase),
                    p.SortOrder,
                    surfaces);
            })
            .ToList();

        return new NavigationResponse(tenantId, products);
    }

    private async Task<AuthTokenResponse> IssueSessionAsync(
        PlatformUser user,
        Guid tenantId,
        IReadOnlyList<string> launchableProductKeys,
        string? userAgent,
        string? ipAddress,
        bool rememberDevice,
        CancellationToken cancellationToken,
        string auditAction = "auth.login")
    {
        var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
        var sessionId = Guid.NewGuid();
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshLifetimeDays = ResolveRefreshTokenLifetimeDays(rememberDevice, settings);
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

        var (accessToken, accessExpires) = tokenService.CreateAccessToken(
            user,
            tenantId,
            sessionId,
            launchableProductKeys,
            settings.AccessTokenMinutes);

        await audit.WriteAsync(
            auditAction,
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

    private async Task<AuthTokenResponse> IssueLocalDevBypassSessionAsync(
        string email,
        Guid? requestedTenantId,
        bool rememberDevice,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            await audit.WriteAsync(
                "local-dev-auth",
                "user",
                normalizedEmail,
                "Denied",
                reasonCode: "invalid_user",
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.invalid_credentials", "Invalid user for local development auth bypass.", 401);
        }

        if (user.IsPlatformAdmin)
        {
            await audit.WriteAsync(
                "local-dev-auth",
                "user",
                user.Id.ToString(),
                "Denied",
                actorUserId: user.Id,
                reasonCode: "platform_admin_forbidden",
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.local_dev_bypass_forbidden", "Platform administrators cannot use local development auth bypass.", 403);
        }

        var tenant = await ResolveTenantAsync(user, requestedTenantId, cancellationToken);
        if (tenant.Status != TenantStatuses.Active)
        {
            await audit.WriteAsync(
                "local-dev-auth",
                "tenant",
                tenant.Id.ToString(),
                "Denied",
                tenantId: tenant.Id,
                actorUserId: user.Id,
                reasonCode: "tenant_suspended",
                cancellationToken: cancellationToken);
            throw new StlApiException("auth.tenant_suspended", "Tenant is not active.", 403);
        }

        var launchableProductKeys = await GetAccessibleProductsAsync(user.IsPlatformAdmin, cancellationToken);
        return await IssueSessionAsync(
            user,
            tenant.Id,
            launchableProductKeys,
            userAgent,
            ipAddress,
            rememberDevice,
            cancellationToken,
            "local-dev-auth");
    }

    private static int ResolveRefreshTokenLifetimeDays(
        bool rememberDevice,
        PlatformSessionSettings settings) =>
        rememberDevice
            ? settings.RememberedRefreshTokenDays
            : settings.RefreshTokenDays;

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

    private static string BuildNavigationRoutePath(string productKey)
    {
        var normalized = ProductKeyAliases.Normalize(productKey);
        var routeSegment = normalized.Equals("fieldcompanion", StringComparison.OrdinalIgnoreCase)
            ? "field-companion"
            : normalized;
        return $"/app/{routeSegment}";
    }

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

    private async Task<Tenant> ResolveTenantAsync(
        PlatformUser user,
        Guid? requestedTenantId,
        CancellationToken cancellationToken)
    {
        var memberships = user.Memberships.Where(m => m.IsActive).ToList();
        var membershipTenantIds = memberships
            .Select(m => m.TenantId)
            .Distinct()
            .ToList();

        if (memberships.Count == 0 && !user.IsPlatformAdmin)
        {
            throw new StlApiException("auth.no_tenant_membership", "User has no active tenant memberships.", 403);
        }

        if (requestedTenantId is Guid tenantId)
        {
            if (!user.IsPlatformAdmin && !membershipTenantIds.Contains(tenantId))
            {
                throw new StlApiException("auth.tenant_forbidden", "User is not a member of the requested tenant.", 403);
            }

            return await db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
                ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        if (membershipTenantIds.Count > 0)
        {
            var membershipTenant = await db.Tenants
                .AsNoTracking()
                .Where(t => membershipTenantIds.Contains(t.Id))
                .OrderBy(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (membershipTenant is not null)
            {
                return membershipTenant;
            }

            if (!user.IsPlatformAdmin)
            {
                throw new StlApiException("auth.no_tenant_membership", "User has no active tenant memberships.", 403);
            }
        }

        return await db.Tenants
            .AsNoTracking()
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
    }

    private Task<IReadOnlyList<string>> GetAccessibleProductsAsync(
        bool isPlatformAdmin,
        CancellationToken cancellationToken) =>
        productAccess.ListAccessibleProductKeysAsync(
            isPlatformAdmin,
            includeWorkers: false,
            cancellationToken);

    private async Task RequireActiveTenantMembershipAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var hasActiveMembership = await db.TenantMemberships.AsNoTracking().AnyAsync(
            membership => membership.UserId == userId
                && membership.TenantId == tenantId
                && membership.IsActive,
            cancellationToken);

        if (!hasActiveMembership)
        {
            throw new StlApiException(
                "auth.tenant_membership_inactive",
                "Your tenant membership is no longer active.",
                403);
        }
    }

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

