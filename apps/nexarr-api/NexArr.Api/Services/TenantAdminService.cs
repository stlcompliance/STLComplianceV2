using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class TenantAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public async Task<PagedResult<TenantDetailResponse>> ListAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Tenant> query = db.Tenants.AsNoTracking();

        if (!principal.IsPlatformAdmin())
        {
            var tenantId = principal.GetTenantId();
            await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
            query = query.Where(t => t.Id == tenantId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantDetailResponse(
                t.Id,
                t.Slug,
                t.DisplayName,
                t.Status,
                t.SubscriptionTier,
                t.BillingCustomerId,
                t.BillingSubscriptionId,
                t.BillingGraceDays,
                t.IsTrial,
                t.IsInternalTenant,
                t.CreatedAt,
                t.ModifiedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<TenantDetailResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<TenantDetailResponse> GetAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        return new TenantDetailResponse(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status,
            tenant.SubscriptionTier,
            tenant.BillingCustomerId,
            tenant.BillingSubscriptionId,
            tenant.BillingGraceDays,
            tenant.IsTrial,
            tenant.IsInternalTenant,
            tenant.CreatedAt,
            tenant.ModifiedAt);
    }

    public async Task<TenantDetailResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var slug = NormalizeSlug(request.Slug);
        if (await db.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            throw new StlApiException("tenant.slug_conflict", "A tenant with this slug already exists.", 409);
        }

        var normalizedTier = NormalizeSubscriptionTier(request.SubscriptionTier);
        var isTrialTenant = request.IsTrial || normalizedTier == TenantSubscriptionTiers.Trial;
        if (isTrialTenant && normalizedTier == TenantSubscriptionTiers.Standard)
        {
            normalizedTier = TenantSubscriptionTiers.Trial;
        }

        var now = DateTimeOffset.UtcNow;
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            DisplayName = NormalizeDisplayName(request.DisplayName),
            Status = isTrialTenant ? TenantStatuses.Trial : TenantStatuses.Active,
            SubscriptionTier = normalizedTier,
            BillingCustomerId = NormalizeOptionalString(request.BillingCustomerId, 128),
            BillingSubscriptionId = NormalizeOptionalString(request.BillingSubscriptionId, 128),
            BillingGraceDays = request.BillingGraceDays,
            IsTrial = isTrialTenant,
            IsInternalTenant = request.IsInternalTenant,
            CreatedAt = now,
            ModifiedAt = now
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.create",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.TenantCreated,
            "tenant",
            tenant.Id.ToString(),
            tenant.CreatedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenant.Id,
                principal.GetUserId(),
                "tenant",
                tenant.Id.ToString(),
                $"Tenant created: {tenant.DisplayName}",
                new Dictionary<string, string>
                {
                    ["slug"] = tenant.Slug,
                    ["status"] = tenant.Status,
                }),
            cancellationToken: cancellationToken);

        return new TenantDetailResponse(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status,
            tenant.SubscriptionTier,
            tenant.BillingCustomerId,
            tenant.BillingSubscriptionId,
            tenant.BillingGraceDays,
            tenant.IsTrial,
            tenant.IsInternalTenant,
            tenant.CreatedAt,
            tenant.ModifiedAt);
    }

    public async Task<TenantDetailResponse> UpdateAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        UpdateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var isPlatformAdmin = principal.IsPlatformAdmin();
        if (isPlatformAdmin)
        {
            await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        }
        else
        {
            await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        if (!isPlatformAdmin)
        {
            if (!IsRenameOnlyUpdate(request, tenant))
            {
                throw new StlApiException(
                    "tenant.rename_only",
                    "Tenant administrators can only rename their tenant.",
                    403);
            }

            var previousDisplayName = tenant.DisplayName;
            tenant.DisplayName = NormalizeDisplayName(request.DisplayName);
            tenant.ModifiedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "tenant.rename",
                "tenant",
                tenant.Id.ToString(),
                "Success",
                tenantId: tenant.Id,
                actorUserId: principal.GetUserId(),
                cancellationToken: cancellationToken);

            await outboxEnqueue.TryEnqueueAsync(
                PlatformOutboxEventKinds.TenantUpdated,
                "tenant",
                tenant.Id.ToString(),
                tenant.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
                new PlatformOutboxPayload(
                    PlatformOutboxRules.DefaultSchemaVersion,
                    tenant.Id,
                    principal.GetUserId(),
                    "tenant",
                    tenant.Id.ToString(),
                    $"Tenant renamed: {tenant.DisplayName}",
                    new Dictionary<string, string>
                    {
                        ["slug"] = tenant.Slug,
                        ["previousDisplayName"] = previousDisplayName,
                        ["displayName"] = tenant.DisplayName,
                        ["status"] = tenant.Status,
                    }),
                cancellationToken: cancellationToken);

            return new TenantDetailResponse(
                tenant.Id,
                tenant.Slug,
                tenant.DisplayName,
                tenant.Status,
                tenant.SubscriptionTier,
                tenant.BillingCustomerId,
                tenant.BillingSubscriptionId,
                tenant.BillingGraceDays,
                tenant.IsTrial,
                tenant.IsInternalTenant,
                tenant.CreatedAt,
                tenant.ModifiedAt);
        }

        tenant.DisplayName = NormalizeDisplayName(request.DisplayName);
        tenant.SubscriptionTier = NormalizeSubscriptionTier(request.SubscriptionTier);
        tenant.BillingCustomerId = NormalizeOptionalString(request.BillingCustomerId, 128);
        tenant.BillingSubscriptionId = NormalizeOptionalString(request.BillingSubscriptionId, 128);
        tenant.BillingGraceDays = request.BillingGraceDays;
        var isTrialTenant = request.IsTrial || tenant.SubscriptionTier == TenantSubscriptionTiers.Trial;
        tenant.IsTrial = isTrialTenant;
        if (isTrialTenant && tenant.Status == TenantStatuses.Active)
        {
            tenant.Status = TenantStatuses.Trial;
        }
        else if (!isTrialTenant && tenant.Status == TenantStatuses.Trial)
        {
            tenant.Status = TenantStatuses.Active;
        }
        tenant.IsInternalTenant = request.IsInternalTenant;
        tenant.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.update",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.TenantUpdated,
            "tenant",
            tenant.Id.ToString(),
            tenant.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenant.Id,
                principal.GetUserId(),
                "tenant",
                tenant.Id.ToString(),
                $"Tenant updated: {tenant.DisplayName}",
                new Dictionary<string, string>
                {
                    ["slug"] = tenant.Slug,
                    ["status"] = tenant.Status,
                }),
            cancellationToken: cancellationToken);

        return new TenantDetailResponse(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status,
            tenant.SubscriptionTier,
            tenant.BillingCustomerId,
            tenant.BillingSubscriptionId,
            tenant.BillingGraceDays,
            tenant.IsTrial,
            tenant.IsInternalTenant,
            tenant.CreatedAt,
            tenant.ModifiedAt);
    }

    public async Task<TenantDetailResponse> UpdateStatusAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        UpdateTenantStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var normalizedStatus = NormalizeStatus(request.Status);

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        if (tenant.Status == TenantStatuses.Archived && normalizedStatus != TenantStatuses.Archived)
        {
            throw new StlApiException("tenant.archived", "Archived tenants cannot be re-enabled or suspended.", 409);
        }

        var previousStatus = tenant.Status;
        tenant.Status = normalizedStatus;
        tenant.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var isArchive = normalizedStatus == TenantStatuses.Archived;
        await audit.WriteAsync(
            isArchive ? "tenant.archive" : "tenant.status_change",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        var eventType = request.Status switch
        {
            TenantStatuses.Active => PlatformOutboxEventKinds.TenantEnabled,
            TenantStatuses.Archived => PlatformOutboxEventKinds.TenantArchived,
            _ => PlatformOutboxEventKinds.TenantDisabled,
        };

        await outboxEnqueue.TryEnqueueAsync(
            eventType,
            "tenant",
            tenant.Id.ToString(),
            tenant.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenant.Id,
                principal.GetUserId(),
                "tenant",
                tenant.Id.ToString(),
                $"Tenant status changed to {tenant.Status}",
                new Dictionary<string, string>
                {
                    ["slug"] = tenant.Slug,
                    ["status"] = tenant.Status,
                    ["previousStatus"] = previousStatus,
                }),
            cancellationToken: cancellationToken);

        return new TenantDetailResponse(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status,
            tenant.SubscriptionTier,
            tenant.BillingCustomerId,
            tenant.BillingSubscriptionId,
            tenant.BillingGraceDays,
            tenant.IsTrial,
            tenant.IsInternalTenant,
            tenant.CreatedAt,
            tenant.ModifiedAt);
    }

    private static string NormalizeSlug(string slug) =>
        slug.Trim().ToLowerInvariant().Replace(' ', '-');

    private static string NormalizeDisplayName(string displayName)
    {
        var normalized = displayName.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException("tenant.invalid_display_name", "Display name is required.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalString(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeSubscriptionTier(string subscriptionTier)
    {
        var normalized = subscriptionTier.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? TenantSubscriptionTiers.Standard : normalized;
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status?.Trim().ToLowerInvariant() ?? string.Empty;
        return normalized switch
        {
            "active" => TenantStatuses.Active,
            "trial" => TenantStatuses.Trial,
            "suspended" or "inactive" => TenantStatuses.Suspended,
            "archived" => TenantStatuses.Archived,
            _ => throw new StlApiException("tenant.invalid_status", "Status must be active, trial, suspended, or archived.", 400),
        };
    }

    private static bool IsRenameOnlyUpdate(UpdateTenantRequest request, Tenant tenant) =>
        NormalizeSubscriptionTier(request.SubscriptionTier) == tenant.SubscriptionTier
        && NormalizeOptionalString(request.BillingCustomerId, 128) == tenant.BillingCustomerId
        && NormalizeOptionalString(request.BillingSubscriptionId, 128) == tenant.BillingSubscriptionId
        && request.BillingGraceDays == tenant.BillingGraceDays
        && request.IsTrial == tenant.IsTrial
        && request.IsInternalTenant == tenant.IsInternalTenant;
}
