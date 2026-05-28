using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformAuditPackageService(
    NexArrDbContext db,
    IPlatformAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public PlatformAuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Sections:
            [
                new("platform_audit_events", "platform_audit_events.json", "Platform audit events", "NexArr control-plane audit trail (optional tenant scope)."),
                new("tenants", "tenants.json", "Tenants", "Tenant registry snapshot."),
                new("tenant_entitlements", "tenant_entitlements.json", "Entitlements", "Tenant product entitlement records."),
                new("product_catalog", "product_catalog.json", "Product catalog", "Suite product catalog entries."),
                new("platform_users", "platform_users.json", "Platform users", "User directory without credential secrets."),
                new("service_clients", "service_clients.json", "Service clients", "Service client registry (no secrets)."),
                new("service_tokens", "service_tokens.json", "Service tokens", "Issued service token metadata (no token material)."),
                new("launch_profiles", "launch_profiles.json", "Launch profiles", "Product launch URL profiles."),
                new("callback_allowlist", "callback_allowlist.json", "Callback allowlist", "Handoff callback allowlist entries."),
            ]);

    public async Task<PagedResult<PlatformAuditEventExportItem>> ListAuditTimelineAsync(
        Guid? scopeTenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 25,
            > 100 => 100,
            _ => pageSize,
        };

        var query = db.AuditEvents.AsNoTracking();
        query = ApplyTenantScope(query, scopeTenantId);
        query = ApplyOccurredAtFilter(query, from, to);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PlatformAuditEventExportItem(
                x.Id,
                x.TenantId,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<PlatformAuditEventExportItem>(
            items,
            page,
            pageSize,
            totalCount,
            page * pageSize < totalCount);
    }

    public async Task<PlatformAuditPackageExportResponse> BuildExportAsync(
        Guid? scopeTenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(scopeTenantId, from, to, cancellationToken);

        await audit.WriteAsync(
            "platform_audit_package.export",
            "platform_audit_package",
            package.PackageId.ToString(),
            "success",
            tenantId: scopeTenantId,
            actorUserId: actorUserId,
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        Guid? scopeTenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await MaterializeExportAsync(scopeTenantId, from, to, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await audit.WriteAsync(
            "platform_audit_package.export",
            "platform_audit_package",
            package.PackageId.ToString(),
            "success",
            tenantId: scopeTenantId,
            actorUserId: actorUserId,
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<PlatformAuditPackageExportResponse> MaterializeExportAsync(
        Guid? scopeTenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await LoadPackageDataAsync(scopeTenantId, from, to, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

    public async Task<byte[]> CreateZipBytesAsync(
        PlatformAuditPackageExportResponse package,
        CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteJsonEntryAsync(archive, "manifest.json", new
            {
                package.PackageId,
                package.ScopeTenantId,
                package.GeneratedAt,
                package.DateRange,
                package.Counts,
                PackageVersion = "1",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "platform_audit_events.json", package.AuditEvents, cancellationToken);
            await WriteJsonEntryAsync(archive, "tenants.json", package.Tenants, cancellationToken);
            await WriteJsonEntryAsync(archive, "tenant_entitlements.json", package.TenantEntitlements, cancellationToken);
            await WriteJsonEntryAsync(archive, "product_catalog.json", package.ProductCatalog, cancellationToken);
            await WriteJsonEntryAsync(archive, "platform_users.json", package.PlatformUsers, cancellationToken);
            await WriteJsonEntryAsync(archive, "service_clients.json", package.ServiceClients, cancellationToken);
            await WriteJsonEntryAsync(archive, "service_tokens.json", package.ServiceTokens, cancellationToken);
            await WriteJsonEntryAsync(archive, "launch_profiles.json", package.LaunchProfiles, cancellationToken);
            await WriteJsonEntryAsync(archive, "callback_allowlist.json", package.CallbackAllowlist, cancellationToken);
        }

        return memory.ToArray();
    }

    private async Task<PlatformAuditPackageExportResponse> LoadPackageDataAsync(
        Guid? scopeTenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents.AsNoTracking();
        auditEventsQuery = ApplyTenantScope(auditEventsQuery, scopeTenantId);
        auditEventsQuery = ApplyOccurredAtFilter(auditEventsQuery, from, to);

        var tenantsQuery = db.Tenants.AsNoTracking();
        if (scopeTenantId is Guid tenantId)
        {
            tenantsQuery = tenantsQuery.Where(x => x.Id == tenantId);
        }

        var entitlementsQuery = db.Entitlements.AsNoTracking();
        if (scopeTenantId is Guid scopedTenant)
        {
            entitlementsQuery = entitlementsQuery.Where(x => x.TenantId == scopedTenant);
        }

        var auditEvents = await auditEventsQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new PlatformAuditEventExportItem(
                x.Id,
                x.TenantId,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var tenants = await tenantsQuery
            .OrderBy(x => x.Slug)
            .Select(x => new PlatformAuditPackageTenantItem(
                x.Id,
                x.Slug,
                x.DisplayName,
                x.Status,
                x.CreatedAt,
                x.ModifiedAt))
            .ToListAsync(cancellationToken);

        var entitlements = await entitlementsQuery
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.ProductKey)
            .Select(x => new PlatformAuditPackageEntitlementItem(
                x.Id,
                x.TenantId,
                x.ProductKey,
                x.Status,
                x.GrantedAt,
                x.RevokedAt))
            .ToListAsync(cancellationToken);

        var products = await db.ProductCatalog
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .Select(x => new PlatformAuditPackageProductItem(
                x.ProductKey,
                x.DisplayName,
                x.IsActive,
                x.SortOrder))
            .ToListAsync(cancellationToken);

        var users = await db.Users
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .Select(x => new PlatformAuditPackageUserItem(
                x.Id,
                x.Email,
                x.DisplayName,
                x.IsActive,
                x.IsPlatformAdmin,
                x.CreatedAt,
                x.ModifiedAt))
            .ToListAsync(cancellationToken);

        var serviceClients = await db.ServiceClients
            .AsNoTracking()
            .OrderBy(x => x.ClientKey)
            .Select(x => new PlatformAuditPackageServiceClientItem(
                x.Id,
                x.ClientKey,
                x.DisplayName,
                x.SourceProductKey,
                x.AllowedProductKeys,
                x.IsActive,
                x.CreatedAt,
                x.ModifiedAt))
            .ToListAsync(cancellationToken);

        var serviceTokensQuery = db.ServiceTokens.AsNoTracking();
        if (scopeTenantId is Guid tokenTenantId)
        {
            serviceTokensQuery = serviceTokensQuery.Where(x => x.TenantId == tokenTenantId);
        }

        var serviceTokens = await serviceTokensQuery
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PlatformAuditPackageServiceTokenItem(
                x.Id,
                x.ServiceClientId,
                x.TenantId,
                x.Jti,
                x.ActionScope,
                x.CreatedAt,
                x.ExpiresAt,
                x.RevokedAt))
            .ToListAsync(cancellationToken);

        var launchProfiles = await db.LaunchProfiles
            .AsNoTracking()
            .OrderBy(x => x.ProductKey)
            .Select(x => new PlatformAuditPackageLaunchProfileItem(
                x.ProductKey,
                x.BaseUrl,
                x.LaunchPath,
                x.IsActive))
            .ToListAsync(cancellationToken);

        var allowlistQuery = db.CallbackAllowlist.AsNoTracking();
        if (scopeTenantId is Guid allowlistTenantId)
        {
            allowlistQuery = allowlistQuery.Where(x => x.TenantId == allowlistTenantId || x.TenantId == null);
        }

        var callbackAllowlist = await allowlistQuery
            .OrderBy(x => x.ProductKey)
            .ThenBy(x => x.UrlPattern)
            .Select(x => new PlatformAuditPackageCallbackAllowlistItem(
                x.Id,
                x.ProductKey,
                x.TenantId,
                x.UrlPattern,
                x.PatternType,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return new PlatformAuditPackageExportResponse(
            Guid.Empty,
            scopeTenantId,
            DateTimeOffset.UtcNow,
            new PlatformAuditPackageDateRangeResponse(from, to),
            new PlatformAuditPackageCountsResponse(
                auditEvents.Count,
                tenants.Count,
                entitlements.Count,
                products.Count,
                users.Count,
                serviceClients.Count,
                serviceTokens.Count,
                launchProfiles.Count,
                callbackAllowlist.Count),
            auditEvents,
            tenants,
            entitlements,
            products,
            users,
            serviceClients,
            serviceTokens,
            launchProfiles,
            callbackAllowlist);
    }

    private static IQueryable<PlatformAuditEvent> ApplyTenantScope(
        IQueryable<PlatformAuditEvent> query,
        Guid? scopeTenantId)
    {
        if (scopeTenantId is Guid tenantId)
        {
            return query.Where(x => x.TenantId == tenantId);
        }

        return query;
    }

    private static IQueryable<PlatformAuditEvent> ApplyOccurredAtFilter(
        IQueryable<PlatformAuditEvent> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static async Task WriteJsonEntryAsync<T>(
        ZipArchive archive,
        string entryName,
        T payload,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await JsonSerializer.SerializeAsync(entryStream, payload, JsonOptions, cancellationToken);
    }

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new StlApiException(
                "platform_audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }

    private static string BuildDateRangeReasonCode(DateTimeOffset? from, DateTimeOffset? to) =>
        from is null && to is null
            ? "all"
            : $"{from:O}|{to:O}";
}
