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
    IOptions<StlJwtOptions> jwtOptions)
{
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

        if (!passwordHasher.Verify(request.Password, user.Credential.PasswordHash))
        {
            await audit.WriteAsync("auth.login", "user", user.Id.ToString(), "Denied", actorUserId: user.Id, reasonCode: "invalid_credentials", cancellationToken: cancellationToken);
            throw new StlApiException("auth.invalid_credentials", "Invalid email or password.", 401);
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

        return await IssueSessionAsync(user, tenantId, entitlements, userAgent, ipAddress, cancellationToken);
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

        return await IssueSessionAsync(session.User, tenantId, entitlements, session.UserAgent, session.IpAddress, cancellationToken);
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
        return await db.TenantMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.IsActive)
            .Join(db.Tenants.AsNoTracking(), m => m.TenantId, t => t.Id, (m, t) => new TenantSummary(t.Id, t.Slug, t.DisplayName, t.Status, m.RoleKey))
            .OrderBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EntitlementSummary>> GetMyEntitlementsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Entitlements
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == EntitlementStatuses.Active)
            .Join(db.ProductCatalog.AsNoTracking(), e => e.ProductKey, p => p.ProductKey, (e, p) => new EntitlementSummary(p.ProductKey, p.DisplayName, e.Status))
            .OrderBy(e => e.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<NavigationResponse> GetNavigationAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var products = await db.Entitlements
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == EntitlementStatuses.Active)
            .Join(db.ProductCatalog.AsNoTracking().Where(p => p.IsActive), e => e.ProductKey, p => p.ProductKey, (e, p) => p)
            .OrderBy(p => p.SortOrder)
            .Select(p => new NavigationItem(
                p.ProductKey,
                p.DisplayName,
                $"/app/{p.ProductKey}",
                p.SortOrder))
            .ToListAsync(cancellationToken);

        return new NavigationResponse(tenantId, products);
    }

    private async Task<AuthTokenResponse> IssueSessionAsync(
        PlatformUser user,
        Guid tenantId,
        IReadOnlyList<string> entitlements,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid();
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays);

        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserId = user.Id,
            RefreshTokenHash = tokenService.HashRefreshToken(refreshToken),
            ActiveTenantId = tenantId,
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
