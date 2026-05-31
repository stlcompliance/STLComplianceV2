using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class LaunchService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    ServiceTokenAdminService serviceTokenAdmin,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue,
    IOptions<StlLaunchOptions> launchOptions)
{
    public async Task<LaunchContextResponse> GetLaunchContextAsync(
        ClaimsPrincipal principal,
        string productKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = productKey.Trim().ToLowerInvariant();
        var userId = principal.GetUserId();
        var tenantId = principal.GetTenantId();

        await authorization.RequireProductLaunchAsync(principal, normalizedKey, tenantId, cancellationToken);

        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId, cancellationToken);
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, cancellationToken);
        var product = await db.ProductCatalog.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == normalizedKey && p.IsActive, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Active product was not found.", 404);

        var profile = await db.LaunchProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == normalizedKey && p.IsActive, cancellationToken);

        var denial = await ResolveLaunchDenialAsync(principal, tenant, normalizedKey, cancellationToken);
        var baseUrl = profile?.BaseUrl ?? string.Empty;
        var launchPath = profile?.LaunchPath ?? "/";
        var launchUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? string.Empty
            : ComposeLaunchUrl(baseUrl, launchPath, null);

        return new LaunchContextResponse(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            user.Id,
            user.Email,
            product.ProductKey,
            product.DisplayName,
            baseUrl,
            launchUrl,
            denial is null,
            denial);
    }

    public async Task<LaunchCatalogResponse> GetLaunchCatalogAsync(
        ClaimsPrincipal principal,
        string? currentProductKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        var userId = principal.GetUserId();
        var tenantId = principal.GetTenantId();
        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId, cancellationToken);
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, cancellationToken);
        var normalizedCurrentProductKey = string.IsNullOrWhiteSpace(currentProductKey)
            ? null
            : currentProductKey.Trim().ToLowerInvariant();

        var launchableProductKeys = await db.LaunchProfiles.AsNoTracking()
            .Where(profile => profile.IsActive && profile.BaseUrl != "")
            .Select(profile => profile.ProductKey)
            .ToListAsync(cancellationToken);

        var productsQuery = db.ProductCatalog.AsNoTracking()
            .Where(product =>
                product.IsActive
                && launchableProductKeys.Contains(product.ProductKey)
                && product.ProductStatus != "worker");

        if (!principal.IsPlatformAdmin())
        {
            var entitledProductKeys = await db.Entitlements.AsNoTracking()
                .Where(entitlement =>
                    entitlement.TenantId == tenantId
                    && entitlement.Status == EntitlementStatuses.Active)
                .Select(entitlement => entitlement.ProductKey)
                .ToListAsync(cancellationToken);

            productsQuery = productsQuery.Where(product => entitledProductKeys.Contains(product.ProductKey));
        }

        var products = await productsQuery
            .OrderBy(product => product.SortOrder)
            .Select(product => new LaunchCatalogItemResponse(
                product.ProductKey,
                product.DisplayName,
                product.ProductCategory,
                product.ProductOwner,
                product.ProductStatus,
                product.ServiceAudience,
                $"/launch/{product.ProductKey}",
                normalizedCurrentProductKey == product.ProductKey))
            .ToListAsync(cancellationToken);

        var generatedAt = DateTimeOffset.UtcNow;
        var catalogVersion = await ResolveLaunchCatalogVersionAsync(
            tenant,
            user,
            launchableProductKeys,
            cancellationToken);

        return new LaunchCatalogResponse(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            user.Id,
            user.Email,
            user.DisplayName,
            normalizedCurrentProductKey,
            catalogVersion,
            generatedAt.AddMinutes(5),
            products,
            generatedAt);
    }

    public async Task<ValidateLaunchResponse> ValidateLaunchAsync(
        ClaimsPrincipal principal,
        ValidateLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        var productKey = request.ProductKey.Trim().ToLowerInvariant();
        var tenantId = request.TenantId ?? principal.GetTenantId();

        if (!principal.IsPlatformAdmin())
        {
            await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        }

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            return new ValidateLaunchResponse(tenantId, productKey, false, "tenant_not_found", null);
        }

        var product = await db.ProductCatalog.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == productKey && p.IsActive, cancellationToken);
        if (product is null)
        {
            return new ValidateLaunchResponse(tenantId, productKey, false, "product_not_found", null);
        }

        var denial = await ResolveLaunchDenialAsync(principal, tenant, productKey, cancellationToken);
        var profile = await db.LaunchProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == productKey && p.IsActive, cancellationToken);
        var launchUrl = profile is null || string.IsNullOrWhiteSpace(profile.BaseUrl)
            ? null
            : ComposeLaunchUrl(profile.BaseUrl, profile.LaunchPath, null);

        return new ValidateLaunchResponse(
            tenant.Id,
            productKey,
            denial is null,
            denial,
            launchUrl);
    }

    public async Task<HandoffCreatedResponse> CreateHandoffAsync(
        ClaimsPrincipal principal,
        CreateHandoffRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = request.ProductKey.Trim().ToLowerInvariant();
        var userId = principal.GetUserId();
        var tenantId = principal.GetTenantId();
        var sessionId = principal.GetSessionId();

        await authorization.RequireProductLaunchAsync(principal, normalizedKey, tenantId, cancellationToken);

        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId, cancellationToken);
        var denial = await ResolveLaunchDenialAsync(principal, tenant, normalizedKey, cancellationToken);
        if (denial is not null)
        {
            await audit.WriteAsync(
                "launch.denied",
                "product",
                normalizedKey,
                "Denied",
                tenantId: tenantId,
                actorUserId: userId,
                reasonCode: denial,
                cancellationToken: cancellationToken);
            await EnqueueLaunchFailedEventAsync(
                normalizedKey,
                tenantId,
                userId,
                "product",
                normalizedKey,
                denial,
                cancellationToken);
            throw new StlApiException("launch.denied", "Product launch is not permitted.", 403, denial);
        }

        if (!string.IsNullOrWhiteSpace(request.CallbackUrl))
        {
            var callbackAllowed = await IsCallbackAllowedAsync(normalizedKey, request.CallbackUrl, tenantId, cancellationToken);
            if (!callbackAllowed)
            {
                await audit.WriteAsync(
                    "launch.handoff.create",
                    "handoff_code",
                    normalizedKey,
                    "Denied",
                    tenantId: tenantId,
                    actorUserId: userId,
                    reasonCode: "callback_not_allowed",
                    cancellationToken: cancellationToken);
                await EnqueueLaunchFailedEventAsync(
                    normalizedKey,
                    tenantId,
                    userId,
                    "handoff_code",
                    normalizedKey,
                    "callback_not_allowed",
                    cancellationToken);
                throw new StlApiException("launch.callback_not_allowed", "Callback URL is not on the product allowlist.", 403);
            }
        }

        var profile = await db.LaunchProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == normalizedKey && p.IsActive, cancellationToken);
        if (profile is null)
        {
            await audit.WriteAsync(
                "launch.handoff.create",
                "product",
                normalizedKey,
                "Denied",
                tenantId: tenantId,
                actorUserId: userId,
                reasonCode: "profile_missing",
                cancellationToken: cancellationToken);
            await EnqueueLaunchFailedEventAsync(
                normalizedKey,
                tenantId,
                userId,
                "product",
                normalizedKey,
                "profile_missing",
                cancellationToken);
            throw new StlApiException("launch.profile_missing", "Launch profile is not configured for this product.", 404);
        }

        var plaintextCode = GenerateHandoffCode();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(launchOptions.Value.HandoffLifetimeMinutes);
        var record = new HandoffCodeRecord
        {
            Id = Guid.NewGuid(),
            CodeHash = HashHandoffCode(plaintextCode),
            UserId = userId,
            TenantId = tenantId,
            SessionId = sessionId,
            TargetProductKey = normalizedKey,
            CallbackUrl = request.CallbackUrl?.Trim(),
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.HandoffCodes.Add(record);
        await db.SaveChangesAsync(cancellationToken);

        var launchUrl = ComposeLaunchUrl(profile.BaseUrl, profile.LaunchPath, plaintextCode);

        await audit.WriteAsync(
            "launch.handoff.create",
            "handoff_code",
            record.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: userId,
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.LaunchSucceeded,
            "handoff_code",
            record.Id.ToString(),
            record.CreatedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenantId,
                userId,
                "handoff_code",
                record.Id.ToString(),
                $"Launch handoff created for {normalizedKey}",
                new Dictionary<string, string>
                {
                    ["productCode"] = normalizedKey,
                    ["handoffId"] = record.Id.ToString(),
                    ["sessionId"] = record.SessionId.ToString(),
                    ["callbackConfigured"] = (!string.IsNullOrWhiteSpace(record.CallbackUrl)).ToString().ToLowerInvariant(),
                }),
            cancellationToken: cancellationToken);

        return new HandoffCreatedResponse(plaintextCode, record.Id, expiresAt, launchUrl);
    }

    public async Task<HandoffRedeemedResponse> RedeemHandoffAsync(
        ClaimsPrincipal principal,
        RedeemHandoffRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.HandoffCode))
        {
            throw new StlApiException("launch.handoff_missing", "Handoff code is required.", 400);
        }

        var record = await db.HandoffCodes
            .Include(h => h.User)
            .Include(h => h.Tenant)
            .FirstOrDefaultAsync(h => h.CodeHash == HashHandoffCode(request.HandoffCode.Trim()), cancellationToken)
            ?? throw new StlApiException("launch.handoff_invalid", "Handoff code is invalid or expired.", 401);

        await RequireHandoffRedeemAuthorityAsync(
            principal,
            request.ServiceToken,
            record.Id,
            record.UserId,
            record.TargetProductKey,
            record.TenantId,
            cancellationToken);

        if (record.RedeemedAt is not null)
        {
            await audit.WriteAsync(
                "launch.handoff.redeem",
                "handoff_code",
                record.Id.ToString(),
                "Denied",
                tenantId: record.TenantId,
                reasonCode: "already_redeemed",
                cancellationToken: cancellationToken);
            await EnqueueHandoffFailedEventAsync(record, "already_redeemed", cancellationToken);
            throw new StlApiException("launch.handoff_already_redeemed", "Handoff code has already been redeemed.", 409);
        }

        if (record.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await audit.WriteAsync(
                "launch.handoff.redeem",
                "handoff_code",
                record.Id.ToString(),
                "Denied",
                tenantId: record.TenantId,
                reasonCode: "expired",
                cancellationToken: cancellationToken);
            await EnqueueHandoffFailedEventAsync(record, "expired", cancellationToken);
            throw new StlApiException("launch.handoff_expired", "Handoff code has expired.", 401);
        }

        if (record.Tenant.Status != TenantStatuses.Active)
        {
            await audit.WriteAsync(
                "launch.handoff.redeem",
                "handoff_code",
                record.Id.ToString(),
                "Denied",
                tenantId: record.TenantId,
                actorUserId: record.UserId,
                reasonCode: "tenant_suspended",
                cancellationToken: cancellationToken);
            await EnqueueHandoffFailedEventAsync(record, "tenant_suspended", cancellationToken);
            throw new StlApiException("launch.tenant_inactive", "Tenant is not active.", 403);
        }

        if (!record.User.IsActive)
        {
            await audit.WriteAsync(
                "launch.handoff.redeem",
                "handoff_code",
                record.Id.ToString(),
                "Denied",
                tenantId: record.TenantId,
                actorUserId: record.UserId,
                reasonCode: "user_inactive",
                cancellationToken: cancellationToken);
            await EnqueueHandoffFailedEventAsync(record, "user_inactive", cancellationToken);
            throw new StlApiException("launch.user_inactive", "User account is inactive.", 403);
        }

        var entitled = await db.Entitlements.AnyAsync(
            e => e.TenantId == record.TenantId
                && e.ProductKey == record.TargetProductKey
                && e.Status == EntitlementStatuses.Active,
            cancellationToken);

        if (!entitled && !record.User.IsPlatformAdmin)
        {
            await audit.WriteAsync(
                "launch.handoff.redeem",
                "handoff_code",
                record.Id.ToString(),
                "Denied",
                tenantId: record.TenantId,
                actorUserId: record.UserId,
                reasonCode: "entitlement_revoked",
                cancellationToken: cancellationToken);
            await EnqueueHandoffFailedEventAsync(record, "entitlement_revoked", cancellationToken);
            throw new StlApiException("launch.entitlement_revoked", "Tenant no longer has entitlement to the target product.", 403);
        }

        record.RedeemedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

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

        await audit.WriteAsync(
            "launch.handoff.redeem",
            "handoff_code",
            record.Id.ToString(),
            "Success",
            tenantId: record.TenantId,
            actorUserId: record.UserId,
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.HandoffRedeemed,
            "handoff_code",
            record.Id.ToString(),
            record.RedeemedAt!.Value.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                record.TenantId,
                record.UserId,
                "handoff_code",
                record.Id.ToString(),
                $"Launch handoff redeemed for {record.TargetProductKey}",
                new Dictionary<string, string>
                {
                    ["productCode"] = record.TargetProductKey,
                    ["handoffId"] = record.Id.ToString(),
                    ["sessionId"] = record.SessionId.ToString(),
                    ["tenantRoleKey"] = tenantRoleKey,
                    ["platformAdmin"] = record.User.IsPlatformAdmin.ToString().ToLowerInvariant(),
                }),
            cancellationToken: cancellationToken);

        return new HandoffRedeemedResponse(
            record.UserId,
            record.User.Email,
            record.User.DisplayName,
            record.TenantId,
            record.Tenant.Slug,
            record.Tenant.DisplayName,
            record.TargetProductKey,
            record.SessionId,
            tenantRoleKey,
            record.User.IsPlatformAdmin,
            entitlements,
            record.CallbackUrl);
    }

    public async Task<ValidateCallbackResponse> ValidateCallbackAsync(
        ClaimsPrincipal principal,
        ValidateCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        var productKey = request.ProductKey.Trim().ToLowerInvariant();
        var tenantId = request.TenantId ?? principal.GetTenantId();

        if (!principal.IsPlatformAdmin())
        {
            await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        }

        var allowed = await IsCallbackAllowedAsync(productKey, request.CallbackUrl, tenantId, cancellationToken);
        return new ValidateCallbackResponse(allowed, allowed ? null : "callback_not_allowed");
    }

    public async Task<bool> IsCallbackAllowedAsync(
        string productKey,
        string callbackUrl,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(callbackUrl.Trim(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var entries = await db.CallbackAllowlist
            .AsNoTracking()
            .Where(e => e.IsActive && e.ProductKey == productKey && (e.TenantId == null || e.TenantId == tenantId))
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return false;
        }

        var normalizedCallback = callbackUrl.Trim();
        foreach (var entry in entries)
        {
            if (MatchesPattern(entry.PatternType, entry.UrlPattern, uri, normalizedCallback))
            {
                return true;
            }
        }

        return false;
    }

    private async Task RequireHandoffRedeemAuthorityAsync(
        ClaimsPrincipal principal,
        string? serviceToken,
        Guid handoffId,
        Guid handoffUserId,
        string targetProductKey,
        Guid handoffTenantId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(serviceToken))
        {
            var validation = await serviceTokenAdmin.ValidateForHandoffRedeemAsync(
                serviceToken,
                cancellationToken);

            if (!validation.IsValid)
            {
                await audit.WriteAsync(
                    "launch.handoff.redeem",
                    "handoff_code",
                    handoffId.ToString(),
                    "Denied",
                    tenantId: handoffTenantId,
                    actorUserId: handoffUserId,
                    reasonCode: validation.ReasonCode ?? "auth.service_token_invalid",
                    cancellationToken: cancellationToken);
                throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401, validation.ReasonCode);
            }

            if (validation.TenantId is Guid scopedTenant && scopedTenant != handoffTenantId)
            {
                await audit.WriteAsync(
                    "launch.handoff.redeem",
                    "handoff_code",
                    handoffId.ToString(),
                    "Denied",
                    tenantId: handoffTenantId,
                    actorUserId: handoffUserId,
                    reasonCode: "auth.tenant_forbidden",
                    cancellationToken: cancellationToken);
                throw new StlApiException("auth.tenant_forbidden", "Service token tenant scope does not match handoff.", 403);
            }

            var sourceProduct = validation.SourceProductKey?.Trim().ToLowerInvariant();
            if (!string.Equals(sourceProduct, targetProductKey, StringComparison.Ordinal)
                && validation.AllowedProductKeys.All(k => !string.Equals(k, targetProductKey, StringComparison.Ordinal)))
            {
                await audit.WriteAsync(
                    "launch.handoff.redeem",
                    "handoff_code",
                    handoffId.ToString(),
                    "Denied",
                    tenantId: handoffTenantId,
                    actorUserId: handoffUserId,
                    reasonCode: "auth.service_token_scope",
                    cancellationToken: cancellationToken);
                throw new StlApiException("auth.service_token_scope", "Service token is not authorized for this product handoff.", 403);
            }

            return;
        }

        if (principal.IsPlatformAdmin())
        {
            return;
        }

        await audit.WriteAsync(
            "launch.handoff.redeem",
            "handoff_code",
            handoffId.ToString(),
            "Denied",
            tenantId: handoffTenantId,
            actorUserId: handoffUserId,
            reasonCode: "auth.forbidden",
            cancellationToken: cancellationToken);
        throw new StlApiException("auth.forbidden", "A valid service token or platform administrator access is required to redeem handoff codes.", 403);
    }

    private async Task<string?> ResolveLaunchDenialAsync(
        ClaimsPrincipal principal,
        Tenant tenant,
        string productKey,
        CancellationToken cancellationToken)
    {
        if (tenant.Status != TenantStatuses.Active)
        {
            return "tenant_suspended";
        }

        if (!principal.IsPlatformAdmin() && !principal.HasProductEntitlement(productKey))
        {
            return "not_entitled";
        }

        if (!principal.IsPlatformAdmin())
        {
            var entitled = await db.Entitlements.AnyAsync(
                e => e.TenantId == tenant.Id
                    && e.ProductKey == productKey
                    && e.Status == EntitlementStatuses.Active,
                cancellationToken);
            if (!entitled)
            {
                return "entitlement_inactive";
            }
        }

        var profileExists = await db.LaunchProfiles.AnyAsync(
            p => p.ProductKey == productKey && p.IsActive && p.BaseUrl != "",
            cancellationToken);
        if (!profileExists)
        {
            return "profile_missing";
        }

        return null;
    }

    private async Task<string> ResolveLaunchCatalogVersionAsync(
        Tenant tenant,
        PlatformUser user,
        IReadOnlyList<string> launchableProductKeys,
        CancellationToken cancellationToken)
    {
        var entitlementVersions = await db.Entitlements.AsNoTracking()
            .Where(e => e.TenantId == tenant.Id)
            .OrderBy(e => e.ProductKey)
            .Select(e => $"{e.ProductKey}:{e.Status}:{e.GrantedAt.ToUnixTimeMilliseconds()}:{(e.RevokedAt == null ? 0 : e.RevokedAt.Value.ToUnixTimeMilliseconds())}")
            .ToListAsync(cancellationToken);

        var productVersions = await db.ProductCatalog.AsNoTracking()
            .Where(p => launchableProductKeys.Contains(p.ProductKey))
            .OrderBy(p => p.ProductKey)
            .Select(p => $"{p.ProductKey}:{p.IsActive}:{p.ProductStatus}:{p.SortOrder}:{p.ProductCategory}:{p.ProductOwner}:{p.ServiceAudience}")
            .ToListAsync(cancellationToken);

        var profileVersions = await db.LaunchProfiles.AsNoTracking()
            .Where(p => launchableProductKeys.Contains(p.ProductKey))
            .OrderBy(p => p.ProductKey)
            .Select(p => $"{p.ProductKey}:{p.IsActive}:{p.BaseUrl}:{p.LaunchPath}:{p.ModifiedAt.ToUnixTimeMilliseconds()}")
            .ToListAsync(cancellationToken);

        var membershipVersions = await db.TenantMemberships.AsNoTracking()
            .Where(m => m.TenantId == tenant.Id && m.UserId == user.Id)
            .OrderBy(m => m.Id)
            .Select(m => $"{m.Id}:{m.IsActive}:{m.RoleKey}:{m.CreatedAt.ToUnixTimeMilliseconds()}")
            .ToListAsync(cancellationToken);

        var versionSource = string.Join(
            "|",
            tenant.Id,
            tenant.Status,
            tenant.ModifiedAt.ToUnixTimeMilliseconds(),
            user.Id,
            user.IsActive,
            user.IsPlatformAdmin,
            user.ModifiedAt.ToUnixTimeMilliseconds(),
            string.Join(",", entitlementVersions),
            string.Join(",", productVersions),
            string.Join(",", profileVersions),
            string.Join(",", membershipVersions));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(versionSource));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private Task<Guid?> EnqueueLaunchFailedEventAsync(
        string productKey,
        Guid tenantId,
        Guid actorUserId,
        string targetType,
        string targetId,
        string reasonCode,
        CancellationToken cancellationToken) =>
        outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.LaunchFailed,
            targetType,
            targetId,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenantId,
                actorUserId,
                targetType,
                targetId,
                $"Launch failed for {productKey}: {reasonCode}",
                new Dictionary<string, string>
                {
                    ["productCode"] = productKey,
                    ["reasonCode"] = reasonCode,
                }),
            cancellationToken: cancellationToken);

    private Task<Guid?> EnqueueHandoffFailedEventAsync(
        HandoffCodeRecord record,
        string reasonCode,
        CancellationToken cancellationToken) =>
        outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.HandoffFailed,
            "handoff_code",
            record.Id.ToString(),
            reasonCode,
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                record.TenantId,
                record.UserId,
                "handoff_code",
                record.Id.ToString(),
                $"Launch handoff failed for {record.TargetProductKey}: {reasonCode}",
                new Dictionary<string, string>
                {
                    ["productCode"] = record.TargetProductKey,
                    ["handoffId"] = record.Id.ToString(),
                    ["sessionId"] = record.SessionId.ToString(),
                    ["reasonCode"] = reasonCode,
                }),
            cancellationToken: cancellationToken);

    private static bool MatchesPattern(string patternType, string pattern, Uri uri, string callbackUrl)
    {
        if (string.Equals(patternType, CallbackPatternTypes.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return callbackUrl.StartsWith(pattern.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        var origin = $"{uri.Scheme}://{uri.Authority}";
        return string.Equals(origin, pattern.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string ComposeLaunchUrl(string baseUrl, string launchPath, string? handoffCode)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var path = launchPath.StartsWith('/') ? launchPath : $"/{launchPath}";
        var url = $"{trimmedBase}{path}";
        if (string.IsNullOrWhiteSpace(handoffCode))
        {
            return url;
        }

        var separator = url.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{url}{separator}handoff={Uri.EscapeDataString(handoffCode)}";
    }

    private static string GenerateHandoffCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string HashHandoffCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash);
    }
}
