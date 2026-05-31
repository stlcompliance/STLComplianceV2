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
        await authorization.RequirePlatformReadAccessAsync(principal, cancellationToken);
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
        tenantId = await authorization.ResolvePlatformReadTenantScopeAsync(principal, tenantId, cancellationToken);
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

    public async Task<PagedResult<LaunchAttemptTimelineItemResponse>> GetLaunchAttemptsAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        Guid? userId,
        string? productKey,
        Guid? correlationId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? result,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        tenantId = await authorization.ResolvePlatformReadTenantScopeAsync(principal, tenantId, cancellationToken);
        var actorUserId = principal.GetUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AuditEvents.AsNoTracking()
            .Where(e => e.Action.StartsWith("launch."));

        if (tenantId is Guid tid)
        {
            query = query.Where(e => e.TenantId == tid);
        }

        if (userId is Guid uid)
        {
            query = query.Where(e => e.ActorUserId == uid);
        }

        if (correlationId is Guid cid)
        {
            query = query.Where(e => e.CorrelationId == cid);
        }

        if (fromUtc is DateTimeOffset from)
        {
            query = query.Where(e => e.OccurredAt >= from);
        }

        if (toUtc is DateTimeOffset to)
        {
            query = query.Where(e => e.OccurredAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = result.Trim().ToLowerInvariant();
            query = query.Where(e => e.Result.ToLower() == normalizedResult);
        }

        if (!string.IsNullOrWhiteSpace(productKey))
        {
            var normalizedKey = productKey.Trim().ToLowerInvariant();
            var handoffIds = await db.HandoffCodes.AsNoTracking()
                .Where(h => h.TargetProductKey == normalizedKey)
                .Select(h => h.Id.ToString())
                .ToListAsync(cancellationToken);
            query = query.Where(e => e.TargetId == normalizedKey
                || (e.TargetType == "handoff_code" && e.TargetId != null && handoffIds.Contains(e.TargetId)));
        }

        var total = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var handoffIdsOnPage = events
            .Where(e => string.Equals(e.TargetType, "handoff_code", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(e.TargetId, out _))
            .Select(e => Guid.Parse(e.TargetId!))
            .Distinct()
            .ToList();
        var handoffs = handoffIdsOnPage.Count == 0
            ? new List<HandoffCodeRecord>()
            : await db.HandoffCodes.AsNoTracking()
                .Where(h => handoffIdsOnPage.Contains(h.Id))
                .ToListAsync(cancellationToken);

        var tenantIds = events
            .Select(e => e.TenantId)
            .Concat(handoffs.Select(h => (Guid?)h.TenantId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var tenants = tenantIds.Count == 0
            ? new List<Tenant>()
            : await db.Tenants.AsNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

        var userIds = events
            .Select(e => e.ActorUserId)
            .Concat(handoffs.Select(h => (Guid?)h.UserId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var users = userIds.Count == 0
            ? new List<PlatformUser>()
            : await db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

        var productKeys = events
            .Where(e => !Guid.TryParse(e.TargetId, out _))
            .Select(e => e.TargetId?.Trim().ToLowerInvariant())
            .Concat(handoffs.Select(h => h.TargetProductKey))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var products = productKeys.Count == 0
            ? new List<ProductCatalogItem>()
            : await db.ProductCatalog.AsNoTracking()
                .Where(p => productKeys.Contains(p.ProductKey))
                .ToListAsync(cancellationToken);

        var items = events.Select(e =>
        {
            var handoff = handoffs.FirstOrDefault(h => string.Equals(h.Id.ToString(), e.TargetId, StringComparison.OrdinalIgnoreCase));
            var resolvedTenantId = e.TenantId ?? handoff?.TenantId;
            var tenant = resolvedTenantId is Guid resolvedTid
                ? tenants.FirstOrDefault(t => t.Id == resolvedTid)
                : null;
            var resolvedUserId = e.ActorUserId ?? handoff?.UserId;
            var user = resolvedUserId is Guid resolvedUid
                ? users.FirstOrDefault(u => u.Id == resolvedUid)
                : null;
            var resolvedProductKey = handoff?.TargetProductKey ?? ResolveProductKeyFromAuditEvent(e);
            var product = string.IsNullOrWhiteSpace(resolvedProductKey)
                ? null
                : products.FirstOrDefault(p => string.Equals(p.ProductKey, resolvedProductKey, StringComparison.OrdinalIgnoreCase));

            return new LaunchAttemptTimelineItemResponse(
                e.Id,
                resolvedTenantId,
                tenant?.Slug,
                tenant?.DisplayName,
                resolvedUserId,
                user?.Email,
                user?.DisplayName,
                resolvedProductKey,
                product?.DisplayName,
                e.Action,
                e.Result,
                e.ReasonCode,
                e.TargetType,
                e.TargetId,
                e.CorrelationId,
                e.OccurredAt,
                ResolveLaunchRemediationHint(e.ReasonCode));
        }).ToList();

        await audit.WriteAsync(
            "platform_admin.launch_attempts.read",
            "platform_admin",
            "launch_attempts",
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new PagedResult<LaunchAttemptTimelineItemResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<PagedResult<TenantOverviewRowResponse>> GetTenantOverviewAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformReadAccessAsync(principal, cancellationToken);
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
        await authorization.RequirePlatformReadAccessAsync(principal, cancellationToken);
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

    public async Task<PagedResult<PlatformUserAccessHistoryItemResponse>> GetUserAccessHistoryAsync(
        ClaimsPrincipal principal,
        Guid userId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformReadAccessAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AuditEvents.AsNoTracking()
            .Where(e => e.ActorUserId == userId
                && (e.Action.StartsWith("auth.") || e.Action.StartsWith("launch.")));

        if (fromUtc is DateTimeOffset from)
        {
            query = query.Where(e => e.OccurredAt >= from);
        }

        if (toUtc is DateTimeOffset to)
        {
            query = query.Where(e => e.OccurredAt <= to);
        }

        var total = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var handoffIds = events
            .Where(e => string.Equals(e.TargetType, "handoff_code", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(e.TargetId, out _))
            .Select(e => Guid.Parse(e.TargetId!))
            .Distinct()
            .ToList();
        var handoffs = handoffIds.Count == 0
            ? new List<HandoffCodeRecord>()
            : await db.HandoffCodes.AsNoTracking()
                .Where(h => handoffIds.Contains(h.Id))
                .ToListAsync(cancellationToken);

        var tenantIds = events.Select(e => e.TenantId)
            .Concat(handoffs.Select(h => (Guid?)h.TenantId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var tenants = tenantIds.Count == 0
            ? new List<Tenant>()
            : await db.Tenants.AsNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var productKeys = events
            .Select(e => ResolveProductKeyFromAuditEvent(e))
            .Concat(handoffs.Select(h => h.TargetProductKey))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var products = productKeys.Count == 0
            ? new List<ProductCatalogItem>()
            : await db.ProductCatalog.AsNoTracking()
                .Where(p => productKeys.Contains(p.ProductKey))
                .ToListAsync(cancellationToken);

        var items = events.Select(e =>
        {
            var handoff = handoffs.FirstOrDefault(h => string.Equals(h.Id.ToString(), e.TargetId, StringComparison.OrdinalIgnoreCase));
            var resolvedTenantId = e.TenantId ?? handoff?.TenantId;
            var tenant = resolvedTenantId is Guid tid ? tenants.FirstOrDefault(t => t.Id == tid) : null;
            var productKey = handoff?.TargetProductKey ?? ResolveProductKeyFromAuditEvent(e);
            var product = string.IsNullOrWhiteSpace(productKey)
                ? null
                : products.FirstOrDefault(p => string.Equals(p.ProductKey, productKey, StringComparison.OrdinalIgnoreCase));

            return new PlatformUserAccessHistoryItemResponse(
                e.Id,
                userId,
                user?.Email,
                user?.DisplayName,
                resolvedTenantId,
                tenant?.Slug,
                e.Action,
                e.Result,
                e.ReasonCode,
                e.TargetType,
                e.TargetId,
                e.CorrelationId,
                e.OccurredAt,
                productKey,
                product?.DisplayName);
        }).ToList();

        await audit.WriteAsync(
            "platform_admin.user_access_history.read",
            "user",
            userId.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new PagedResult<PlatformUserAccessHistoryItemResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public Task<PagedResult<PlatformUserAccessHistoryItemResponse>> GetUserLoginHistoryAsync(
        ClaimsPrincipal principal,
        Guid userId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        GetUserAccessHistoryByActionPrefixAsync(
            principal,
            userId,
            "auth.",
            "platform_admin.user_login_history.read",
            fromUtc,
            toUtc,
            page,
            pageSize,
            cancellationToken);

    public Task<PagedResult<PlatformUserAccessHistoryItemResponse>> GetUserLaunchHistoryAsync(
        ClaimsPrincipal principal,
        Guid userId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        GetUserAccessHistoryByActionPrefixAsync(
            principal,
            userId,
            "launch.",
            "platform_admin.user_launch_history.read",
            fromUtc,
            toUtc,
            page,
            pageSize,
            cancellationToken);

    private async Task<PagedResult<PlatformUserAccessHistoryItemResponse>> GetUserAccessHistoryByActionPrefixAsync(
        ClaimsPrincipal principal,
        Guid userId,
        string actionPrefix,
        string auditAction,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await authorization.RequirePlatformReadAccessAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AuditEvents.AsNoTracking()
            .Where(e => e.ActorUserId == userId
                && e.Action.StartsWith(actionPrefix));

        if (fromUtc is DateTimeOffset from)
        {
            query = query.Where(e => e.OccurredAt >= from);
        }

        if (toUtc is DateTimeOffset to)
        {
            query = query.Where(e => e.OccurredAt <= to);
        }

        var total = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var handoffIds = events
            .Where(e => string.Equals(e.TargetType, "handoff_code", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(e.TargetId, out _))
            .Select(e => Guid.Parse(e.TargetId!))
            .Distinct()
            .ToList();
        var handoffs = handoffIds.Count == 0
            ? new List<HandoffCodeRecord>()
            : await db.HandoffCodes.AsNoTracking()
                .Where(h => handoffIds.Contains(h.Id))
                .ToListAsync(cancellationToken);

        var tenantIds = events.Select(e => e.TenantId)
            .Concat(handoffs.Select(h => (Guid?)h.TenantId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var tenants = tenantIds.Count == 0
            ? new List<Tenant>()
            : await db.Tenants.AsNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var productKeys = events
            .Select(ResolveProductKeyFromAuditEvent)
            .Concat(handoffs.Select(h => h.TargetProductKey))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var products = productKeys.Count == 0
            ? new List<ProductCatalogItem>()
            : await db.ProductCatalog.AsNoTracking()
                .Where(p => productKeys.Contains(p.ProductKey))
                .ToListAsync(cancellationToken);

        var items = events.Select(e =>
        {
            var handoff = handoffs.FirstOrDefault(h => string.Equals(h.Id.ToString(), e.TargetId, StringComparison.OrdinalIgnoreCase));
            var resolvedTenantId = e.TenantId ?? handoff?.TenantId;
            var tenant = resolvedTenantId is Guid tid ? tenants.FirstOrDefault(t => t.Id == tid) : null;
            var productKey = handoff?.TargetProductKey ?? ResolveProductKeyFromAuditEvent(e);
            var product = string.IsNullOrWhiteSpace(productKey)
                ? null
                : products.FirstOrDefault(p => string.Equals(p.ProductKey, productKey, StringComparison.OrdinalIgnoreCase));

            return new PlatformUserAccessHistoryItemResponse(
                e.Id,
                userId,
                user?.Email,
                user?.DisplayName,
                resolvedTenantId,
                tenant?.Slug,
                e.Action,
                e.Result,
                e.ReasonCode,
                e.TargetType,
                e.TargetId,
                e.CorrelationId,
                e.OccurredAt,
                productKey,
                product?.DisplayName);
        }).ToList();

        await audit.WriteAsync(
            auditAction,
            "user",
            userId.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new PagedResult<PlatformUserAccessHistoryItemResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<PagedResult<PlatformUserIdentityAuditHistoryItemResponse>> GetUserIdentityAuditHistoryAsync(
        ClaimsPrincipal principal,
        Guid userId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformReadAccessAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var userIdText = userId.ToString();
        var tenantMembershipTargetIdsForUser = db.TenantMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.Id.ToString());
        var query = db.AuditEvents.AsNoTracking()
            .Where(e =>
                (e.TargetType == "user" && e.TargetId == userIdText && e.Action.StartsWith("user."))
                || (e.TargetType == "platform_role_assignment" && e.TargetId == userIdText && e.Action.StartsWith("platform.role."))
                || (e.TargetType == "tenant_membership" && e.Action.StartsWith("tenant.membership_") && e.TargetId != null && tenantMembershipTargetIdsForUser.Contains(e.TargetId)));

        if (fromUtc is DateTimeOffset from)
        {
            query = query.Where(e => e.OccurredAt >= from);
        }

        if (toUtc is DateTimeOffset to)
        {
            query = query.Where(e => e.OccurredAt <= to);
        }

        var total = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var membershipTargetIds = events
            .Where(e => e.TargetType == "tenant_membership" && Guid.TryParse(e.TargetId, out _))
            .Select(e => Guid.Parse(e.TargetId!))
            .Distinct()
            .ToList();
        var membershipRecords = membershipTargetIds.Count == 0
            ? new List<TenantMembership>()
            : await db.TenantMemberships.AsNoTracking()
                .Where(m => membershipTargetIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

        var tenantIds = events
            .Select(e => e.TenantId)
            .Concat(membershipRecords.Select(m => (Guid?)m.TenantId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var tenants = tenantIds.Count == 0
            ? new List<Tenant>()
            : await db.Tenants.AsNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

        var actorIds = events
            .Select(e => e.ActorUserId)
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var actors = actorIds.Count == 0
            ? new List<PlatformUser>()
            : await db.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

        var subjectUser = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var items = events.Select(e =>
        {
            Guid? resolvedTenantId = e.TenantId;
            if (resolvedTenantId is null && e.TargetType == "tenant_membership")
            {
                var membership = membershipRecords.FirstOrDefault(m => string.Equals(m.Id.ToString(), e.TargetId, StringComparison.OrdinalIgnoreCase));
                resolvedTenantId = membership?.TenantId;
            }

            var tenant = resolvedTenantId is Guid tid
                ? tenants.FirstOrDefault(t => t.Id == tid)
                : null;
            var actor = e.ActorUserId is Guid aid
                ? actors.FirstOrDefault(u => u.Id == aid)
                : null;

            return new PlatformUserIdentityAuditHistoryItemResponse(
                e.Id,
                userId,
                subjectUser?.Email,
                subjectUser?.DisplayName,
                resolvedTenantId,
                tenant?.Slug,
                e.ActorUserId,
                actor?.Email,
                actor?.DisplayName,
                e.Action,
                e.Result,
                e.ReasonCode,
                e.TargetType,
                e.TargetId,
                e.CorrelationId,
                e.OccurredAt);
        }).ToList();

        await audit.WriteAsync(
            "platform_admin.user_identity_audit_history.read",
            "user",
            userIdText,
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return new PagedResult<PlatformUserIdentityAuditHistoryItemResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    private static string? ResolveProductKeyFromAuditEvent(PlatformAuditEvent auditEvent)
    {
        if (!Guid.TryParse(auditEvent.TargetId, out _))
        {
            return auditEvent.TargetId?.Trim().ToLowerInvariant();
        }

        return null;
    }

    private static string? ResolveLaunchRemediationHint(string? reasonCode) =>
        reasonCode switch
        {
            "callback_not_allowed" => "Add or correct the product callback allowlist entry for this tenant and environment.",
            "not_entitled" or "entitlement_inactive" or "entitlement_revoked" => "Grant or reactivate the tenant entitlement for the requested product.",
            "tenant_suspended" => "Reactivate the tenant before retrying product launch.",
            "user_inactive" => "Reactivate the user account before retrying product launch.",
            "profile_missing" => "Configure an active launch profile with a base URL for the product.",
            "already_redeemed" => "Start a new launch; handoff codes are one-time use.",
            "expired" => "Start a new launch; the previous handoff code expired.",
            "service_token_invalid" or "auth.service_token_invalid" => "Rotate or reissue the product service token.",
            "auth.service_token_scope" => "Update the service client audience or allowed product scope.",
            "auth.tenant_forbidden" => "Use a service token scoped to the handoff tenant.",
            "auth.forbidden" => "Redeem with a valid product service token or platform administrator account.",
            _ => null
        };

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
