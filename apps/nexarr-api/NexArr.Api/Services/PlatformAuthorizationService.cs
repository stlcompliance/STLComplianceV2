using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformAuthorizationService(NexArrDbContext db, IConfiguration configuration)
{
    private const int DefaultPlatformAdminSessionTimeoutMinutes = 60;

    private static readonly HashSet<string> PlatformReadRoleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "platform_support",
        "read_only_auditor",
        "platform_owner",
    };

    private static readonly HashSet<string> PlatformOwnerRoleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "platform_owner",
    };

    public async Task RequireActiveSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        Guid userId;
        Guid tenantId;
        Guid sessionId;
        try
        {
            userId = principal.GetUserId();
            tenantId = principal.GetTenantId();
            sessionId = principal.GetSessionId();
        }
        catch (InvalidOperationException)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        var session = await db.UserSessions.AsNoTracking()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Select(s => new
            {
                s.ActiveTenantId,
                s.ExpiresAt,
                s.RevokedAt,
                UserIsActive = s.User.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null || session.RevokedAt is not null)
        {
            throw new StlApiException("auth.session_revoked", "Session has ended. Sign in again.", 401);
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new StlApiException("auth.session_expired", "Session expired. Sign in again.", 401);
        }

        if (!session.UserIsActive)
        {
            throw new StlApiException("auth.user_inactive", "User account is inactive.", 403);
        }

        if (session.ActiveTenantId != tenantId)
        {
            throw new StlApiException("auth.tenant_forbidden", "Session tenant does not match the requested tenant.", 403);
        }
    }

    public async Task RequirePlatformAdminAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        if (!principal.IsPlatformAdmin())
        {
            throw new StlApiException("auth.forbidden", "Platform administrator access is required.", 403);
        }

        var userId = principal.GetUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        if (user is null)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        await RequireRecentPlatformAdminSessionAsync(principal, userId, cancellationToken);
    }

    public async Task RequirePlatformReadAccessAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        if (user is null)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        await RequireRecentPlatformAdminSessionAsync(principal, userId, cancellationToken);

        if (principal.IsPlatformAdmin() || user.IsPlatformAdmin)
        {
            return;
        }

        var hasReadRole = await db.PlatformRoleAssignments.AsNoTracking().AnyAsync(
            x => x.UserId == userId
                && x.TenantId == null
                && PlatformReadRoleKeys.Contains(x.RoleKey),
            cancellationToken);

        if (!hasReadRole)
        {
            throw new StlApiException("auth.forbidden", "Platform read access is required.", 403);
        }
    }

    public async Task<Guid?> ResolvePlatformReadTenantScopeAsync(
        ClaimsPrincipal principal,
        Guid? requestedTenantId,
        CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        if (user is null)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        await RequireRecentPlatformAdminSessionAsync(principal, userId, cancellationToken);

        if (principal.IsPlatformAdmin() || user.IsPlatformAdmin)
        {
            return requestedTenantId;
        }

        var hasGlobalReadRole = await db.PlatformRoleAssignments.AsNoTracking().AnyAsync(
            x => x.UserId == userId
                 && x.TenantId == null
                 && PlatformReadRoleKeys.Contains(x.RoleKey),
            cancellationToken);
        if (hasGlobalReadRole)
        {
            return requestedTenantId;
        }

        if (requestedTenantId is not Guid scopedTenantId)
        {
            throw new StlApiException(
                "auth.tenant_scope_required",
                "Tenant scope is required for this platform read role.",
                403);
        }

        var hasTenantScopedReadRole = await db.PlatformRoleAssignments.AsNoTracking().AnyAsync(
            x => x.UserId == userId
                 && x.TenantId == scopedTenantId
                 && PlatformReadRoleKeys.Contains(x.RoleKey),
            cancellationToken);
        if (!hasTenantScopedReadRole)
        {
            throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
        }

        return scopedTenantId;
    }

    public async Task RequirePlatformOwnerAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        if (user is null)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        await RequireRecentPlatformAdminSessionAsync(principal, userId, cancellationToken);

        var hasOwnerRole = await db.PlatformRoleAssignments.AsNoTracking().AnyAsync(
            x => x.UserId == userId
                && x.TenantId == null
                && PlatformOwnerRoleKeys.Contains(x.RoleKey),
            cancellationToken);

        if (!hasOwnerRole)
        {
            throw new StlApiException("auth.forbidden", "Platform owner access is required.", 403);
        }
    }

    private async Task RequireRecentPlatformAdminSessionAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!TryGetPlatformAdminSessionTimeoutMinutes(configuration, out var timeoutMinutes))
        {
            return;
        }

        Guid sessionId;
        try
        {
            sessionId = principal.GetSessionId();
        }
        catch (InvalidOperationException)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        var session = await db.UserSessions.AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == sessionId && s.UserId == userId && s.RevokedAt == null,
                cancellationToken);
        if (session is null)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        var now = DateTimeOffset.UtcNow;
        if (session.ExpiresAt <= now)
        {
            throw new StlApiException("auth.session_expired", "Session expired. Sign in again.", 401);
        }

        if (session.CreatedAt.AddMinutes(timeoutMinutes) <= now)
        {
            throw new StlApiException("auth.admin_session_timeout", "Admin session timed out. Sign in again.", 401);
        }
    }

    private static bool TryGetPlatformAdminSessionTimeoutMinutes(IConfiguration configuration, out int timeoutMinutes)
    {
        var configuredValue =
            configuration["AUTH_PLATFORM_ADMIN_SESSION_TIMEOUT_MINUTES"]
            ?? configuration["Auth:PlatformAdminSessionTimeoutMinutes"];

        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            timeoutMinutes = DefaultPlatformAdminSessionTimeoutMinutes;
            return true;
        }

        if (!int.TryParse(configuredValue, out timeoutMinutes))
        {
            timeoutMinutes = DefaultPlatformAdminSessionTimeoutMinutes;
            return true;
        }

        if (timeoutMinutes <= 0)
        {
            return false;
        }

        return true;
    }

    public async Task RequireNexArrAccessAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        if (principal.IsPlatformAdmin() || principal.HasProductEntitlement("nexarr"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "NexArr entitlement is required.", 403);
    }

    public async Task RequireTenantAccessAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        bool allowTenantAdmin = false,
        CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var jwtTenantId = principal.GetTenantId();
        if (jwtTenantId != tenantId)
        {
            throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
        }

        if (!allowTenantAdmin)
        {
            throw new StlApiException("auth.forbidden", "Platform administrator access is required.", 403);
        }

        var userId = principal.GetUserId();
        var membership = await db.TenantMemberships.AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.TenantId == tenantId && m.IsActive,
                cancellationToken);

        if (membership is null || !IsTenantAdminRole(membership.RoleKey))
        {
            throw new StlApiException("auth.forbidden", "Tenant administrator access is required.", 403);
        }
    }

    public static bool IsTenantAdminRole(string roleKey) =>
        string.Equals(roleKey, "tenant_admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(roleKey, "platform_admin", StringComparison.OrdinalIgnoreCase);

    public async Task RequireProductLaunchAsync(
        ClaimsPrincipal principal,
        string productKey,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await RequireActiveSessionAsync(principal, cancellationToken);

        if (principal.IsPlatformAdmin() || principal.HasProductEntitlement(productKey))
        {
            if (principal.IsPlatformAdmin())
            {
                return;
            }

            var jwtTenantId = principal.GetTenantId();
            if (jwtTenantId != tenantId)
            {
                throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
            }

            return;
        }

        throw new StlApiException("auth.forbidden", "Product entitlement is required to launch this product.", 403);
    }
}
