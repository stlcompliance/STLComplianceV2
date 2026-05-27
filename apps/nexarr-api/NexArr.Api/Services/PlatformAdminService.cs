using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PlatformAdminDashboardResponse> GetDashboardAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;
        var dayAgo = now.AddHours(-24);

        var tenantCount = await db.Tenants.CountAsync(cancellationToken);
        var activeTenantCount = await db.Tenants.CountAsync(t => t.Status == TenantStatuses.Active, cancellationToken);
        var productCount = await db.ProductCatalog.CountAsync(cancellationToken);
        var activeProductCount = await db.ProductCatalog.CountAsync(p => p.IsActive, cancellationToken);
        var totalEntitlementCount = await db.Entitlements.CountAsync(cancellationToken);
        var activeEntitlementCount = await db.Entitlements.CountAsync(
            e => e.Status == EntitlementStatuses.Active,
            cancellationToken);
        var serviceClientCount = await db.ServiceClients.CountAsync(c => c.IsActive, cancellationToken);
        var activeServiceTokenCount = await db.ServiceTokens.CountAsync(
            t => t.RevokedAt == null && t.ExpiresAt > now,
            cancellationToken);
        var launchProfileCount = await db.LaunchProfiles.CountAsync(p => p.IsActive, cancellationToken);
        var pendingHandoffCount = await db.HandoffCodes.CountAsync(
            h => h.RedeemedAt == null && h.ExpiresAt > now,
            cancellationToken);
        var expiredUnredeemedHandoffCount = await db.HandoffCodes.CountAsync(
            h => h.RedeemedAt == null && h.ExpiresAt <= now,
            cancellationToken);
        var auditEventsLast24Hours = await db.AuditEvents.CountAsync(
            e => e.OccurredAt >= dayAgo,
            cancellationToken);

        await audit.WriteAsync(
            "platform_admin.dashboard.read",
            "platform_admin",
            "dashboard",
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new PlatformAdminDashboardResponse(
            tenantCount,
            activeTenantCount,
            productCount,
            activeProductCount,
            activeEntitlementCount,
            totalEntitlementCount,
            serviceClientCount,
            activeServiceTokenCount,
            launchProfileCount,
            pendingHandoffCount,
            expiredUnredeemedHandoffCount,
            auditEventsLast24Hours,
            now);
    }

    public async Task<LaunchDiagnosticsResponse> GetLaunchDiagnosticsAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        string? productKey,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var tenantsQuery = db.Tenants.AsNoTracking();
        if (tenantId is Guid tid)
        {
            tenantsQuery = tenantsQuery.Where(t => t.Id == tid);
        }

        var productsQuery = db.ProductCatalog.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(productKey))
        {
            var normalizedKey = productKey.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(p => p.ProductKey == normalizedKey);
        }

        var tenants = await tenantsQuery.OrderBy(t => t.DisplayName).ToListAsync(cancellationToken);
        var products = await productsQuery.OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);

        var entitlements = await db.Entitlements.AsNoTracking()
            .Where(e => e.Status == EntitlementStatuses.Active)
            .ToListAsync(cancellationToken);

        var profiles = await db.LaunchProfiles.AsNoTracking().ToListAsync(cancellationToken);
        var allowlistEntries = await db.CallbackAllowlist.AsNoTracking()
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        var handoffRecords = await db.HandoffCodes.AsNoTracking().ToListAsync(cancellationToken);
        var handoffStats = handoffRecords
            .GroupBy(h => new { h.TenantId, h.TargetProductKey })
            .Select(g => new
            {
                g.Key.TenantId,
                g.Key.TargetProductKey,
                Pending = g.Count(h => h.RedeemedAt == null && h.ExpiresAt > now),
                Expired = g.Count(h => h.RedeemedAt == null && h.ExpiresAt <= now)
            })
            .ToList();

        var rows = new List<LaunchDiagnosticRowResponse>();
        foreach (var tenant in tenants)
        {
            foreach (var product in products)
            {
                var entitled = entitlements.Any(
                    e => e.TenantId == tenant.Id && e.ProductKey == product.ProductKey);
                var profile = profiles.FirstOrDefault(p => p.ProductKey == product.ProductKey);
                var hasProfile = profile is not null;
                var profileActive = profile is { IsActive: true } && !string.IsNullOrWhiteSpace(profile.BaseUrl);
                var allowlistCount = allowlistEntries
                    .Count(a => a.ProductKey == product.ProductKey
                        && (a.TenantId == null || a.TenantId == tenant.Id));
                var handoff = handoffStats.FirstOrDefault(
                    h => h.TenantId == tenant.Id && h.TargetProductKey == product.ProductKey);

                var readiness = ResolveLaunchReadiness(tenant, entitled, profileActive);
                rows.Add(new LaunchDiagnosticRowResponse(
                    tenant.Id,
                    tenant.Slug,
                    tenant.DisplayName,
                    tenant.Status,
                    product.ProductKey,
                    product.DisplayName,
                    entitled,
                    hasProfile,
                    profileActive,
                    allowlistCount,
                    handoff?.Pending ?? 0,
                    handoff?.Expired ?? 0,
                    readiness));
            }
        }

        var pagedRows = rows
            .OrderBy(r => r.TenantDisplayName)
            .ThenBy(r => r.ProductKey)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var issues = BuildLaunchIssues(tenants, products, entitlements, profiles);

        await audit.WriteAsync(
            "platform_admin.launch_diagnostics.read",
            "platform_admin",
            "launch_diagnostics",
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new LaunchDiagnosticsResponse(pagedRows, issues, now);
    }

    public async Task<PagedResult<TenantOverviewRowResponse>> GetTenantOverviewAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Tenants.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var tenants = await query
            .OrderBy(t => t.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var tenantIds = tenants.Select(t => t.Id).ToHashSet();
        var activeEntitlements = await db.Entitlements.AsNoTracking()
            .Where(e => e.Status == EntitlementStatuses.Active)
            .ToListAsync(cancellationToken);
        var entitlementCounts = activeEntitlements
            .Where(e => tenantIds.Contains(e.TenantId))
            .GroupBy(e => e.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToList();

        var memberships = await db.TenantMemberships.AsNoTracking()
            .Where(m => m.IsActive)
            .ToListAsync(cancellationToken);
        var membershipCounts = memberships
            .Where(m => tenantIds.Contains(m.TenantId))
            .GroupBy(m => m.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToList();

        var items = tenants.Select(t =>
        {
            var entCount = entitlementCounts.FirstOrDefault(e => e.TenantId == t.Id)?.Count ?? 0;
            var memCount = membershipCounts.FirstOrDefault(m => m.TenantId == t.Id)?.Count ?? 0;
            return new TenantOverviewRowResponse(
                t.Id,
                t.Slug,
                t.DisplayName,
                t.Status,
                entCount,
                memCount,
                t.CreatedAt);
        }).ToList();

        await audit.WriteAsync(
            "platform_admin.overview.tenants.read",
            "platform_admin",
            "tenant_overview",
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new PagedResult<TenantOverviewRowResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<IReadOnlyList<ProductOverviewRowResponse>> GetProductOverviewAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        var products = await db.ProductCatalog.AsNoTracking()
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        var entitlementCounts = await db.Entitlements.AsNoTracking()
            .Where(e => e.Status == EntitlementStatuses.Active)
            .GroupBy(e => e.ProductKey)
            .Select(g => new { ProductKey = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var profiles = await db.LaunchProfiles.AsNoTracking().ToListAsync(cancellationToken);

        var items = products.Select(p =>
        {
            var profile = profiles.FirstOrDefault(lp => lp.ProductKey == p.ProductKey);
            var entCount = entitlementCounts.FirstOrDefault(e => e.ProductKey == p.ProductKey)?.Count ?? 0;
            return new ProductOverviewRowResponse(
                p.ProductKey,
                p.DisplayName,
                p.IsActive,
                entCount,
                profile is not null,
                profile is { IsActive: true } && !string.IsNullOrWhiteSpace(profile.BaseUrl),
                profile?.BaseUrl);
        }).ToList();

        await audit.WriteAsync(
            "platform_admin.overview.products.read",
            "platform_admin",
            "product_overview",
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return items;
    }

    private static string ResolveLaunchReadiness(Tenant tenant, bool entitled, bool profileActive)
    {
        if (tenant.Status != TenantStatuses.Active)
        {
            return "tenant_suspended";
        }

        if (!entitled)
        {
            return "not_entitled";
        }

        if (!profileActive)
        {
            return "profile_missing";
        }

        return "ready";
    }

    private static IReadOnlyList<LaunchDiagnosticIssueResponse> BuildLaunchIssues(
        IReadOnlyList<Tenant> tenants,
        IReadOnlyList<ProductCatalogItem> products,
        IReadOnlyList<TenantProductEntitlement> entitlements,
        IReadOnlyList<ProductLaunchProfile> profiles)
    {
        var issues = new List<LaunchDiagnosticIssueResponse>();

        foreach (var product in products)
        {
            var profile = profiles.FirstOrDefault(p => p.ProductKey == product.ProductKey);
            if (profile is null || !profile.IsActive || string.IsNullOrWhiteSpace(profile.BaseUrl))
            {
                issues.Add(new LaunchDiagnosticIssueResponse(
                    "profile_missing",
                    "error",
                    $"Product '{product.DisplayName}' has no active launch profile.",
                    null,
                    null,
                    product.ProductKey));
            }
        }

        foreach (var tenant in tenants.Where(t => t.Status == TenantStatuses.Active))
        {
            foreach (var product in products)
            {
                var entitled = entitlements.Any(
                    e => e.TenantId == tenant.Id && e.ProductKey == product.ProductKey);
                if (!entitled)
                {
                    issues.Add(new LaunchDiagnosticIssueResponse(
                        "not_entitled",
                        "warning",
                        $"Tenant '{tenant.DisplayName}' is not entitled to '{product.DisplayName}'.",
                        tenant.Id,
                        tenant.Slug,
                        product.ProductKey));
                }
            }
        }

        return issues
            .OrderByDescending(i => i.Severity == "error")
            .ThenBy(i => i.TenantSlug)
            .ThenBy(i => i.ProductKey)
            .Take(50)
            .ToList();
    }
}
