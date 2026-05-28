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
            PackageVersion: "2",
            Sections:
            [
                new("platform_audit_events", "platform_audit_events.json", "Platform audit events", "NexArr control-plane audit trail (JSON)."),
                new("platform_audit_events_csv", "platform_audit_events.csv", "Platform audit events (CSV)", "Same audit events in CSV for spreadsheets."),
                new("tenants", "tenants.json", "Tenants", "Tenant registry snapshot."),
                new("tenant_entitlements", "tenant_entitlements.json", "Entitlements", "Tenant product entitlement records."),
                new("product_catalog", "product_catalog.json", "Product catalog", "Suite product catalog entries."),
                new("platform_users", "platform_users.json", "Platform users", "User directory without credential secrets."),
                new("service_clients", "service_clients.json", "Service clients", "Service client registry (no secrets)."),
                new("service_tokens", "service_tokens.json", "Service tokens", "Issued service token metadata (no token material)."),
                new("launch_profiles", "launch_profiles.json", "Launch profiles", "Product launch URL profiles."),
                new("callback_allowlist", "callback_allowlist.json", "Callback allowlist", "Handoff callback allowlist entries."),
            ]);

    public async Task<PlatformAuditPackageFilterOptionsResponse> GetFilterOptionsAsync(
        PlatformAuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = db.AuditEvents.AsNoTracking();
        query = ApplyTenantScope(query, filter.TenantId);

        var actions = await query
            .Select(x => x.Action)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var results = await query
            .Select(x => x.Result)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var targetTypes = await query
            .Select(x => x.TargetType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var productKeys = await db.ProductCatalog
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => x.ProductKey)
            .ToListAsync(cancellationToken);

        return new PlatformAuditPackageFilterOptionsResponse(actions, results, targetTypes, productKeys);
    }

    public async Task<PlatformAuditPackageExportSummaryResponse> GetExportSummaryAsync(
        PlatformAuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(filter, cancellationToken);

        var byResult = package.AuditEvents
            .GroupBy(x => x.Result, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PlatformAuditPackageBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var byAction = package.AuditEvents
            .GroupBy(x => x.Action, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PlatformAuditPackageBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(x => x.Count)
            .Take(15)
            .ToList();

        return new PlatformAuditPackageExportSummaryResponse(
            MapAppliedFilters(filter),
            package.Counts,
            byResult,
            byAction,
            DateTimeOffset.UtcNow);
    }

    public async Task<PagedResult<PlatformAuditEventExportItem>> ListAuditTimelineAsync(
        PlatformAuditPackageFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 25,
            > 100 => 100,
            _ => pageSize,
        };

        var query = db.AuditEvents.AsNoTracking();
        query = ApplyAuditEventFilters(query, filter);

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
        PlatformAuditPackageFilter filter,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(filter, cancellationToken);

        await audit.WriteAsync(
            "platform_audit_package.export",
            "platform_audit_package",
            package.PackageId.ToString(),
            "success",
            tenantId: filter.TenantId,
            actorUserId: actorUserId,
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        PlatformAuditPackageFilter filter,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await MaterializeExportAsync(filter, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await audit.WriteAsync(
            "platform_audit_package.export",
            "platform_audit_package",
            package.PackageId.ToString(),
            "success",
            tenantId: filter.TenantId,
            actorUserId: actorUserId,
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<byte[]> ExportAuditEventsCsvAsync(
        PlatformAuditPackageFilter filter,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(filter, cancellationToken);
        var csvBytes = PlatformAuditPackageCsvWriter.WriteAuditEvents(package.AuditEvents);

        await audit.WriteAsync(
            "platform_audit_package.export_csv",
            "platform_audit_package",
            Guid.NewGuid().ToString(),
            "success",
            tenantId: filter.TenantId,
            actorUserId: actorUserId,
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return csvBytes;
    }

    public async Task<PlatformAuditPackageExportResponse> MaterializeExportAsync(
        PlatformAuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(filter, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

    public static PlatformAuditPackageFilter FromJob(PlatformAuditPackageGenerationJob job)
    {
        if (string.IsNullOrWhiteSpace(job.FilterJson))
        {
            return new PlatformAuditPackageFilter(job.ScopeTenantId, job.FromUtc, job.ToUtc);
        }

        return JsonSerializer.Deserialize<PlatformAuditPackageFilter>(job.FilterJson, JsonOptions)
            ?? new PlatformAuditPackageFilter(job.ScopeTenantId, job.FromUtc, job.ToUtc);
    }

    public static string SerializeFilter(PlatformAuditPackageFilter filter) =>
        JsonSerializer.Serialize(filter, JsonOptions);

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
                package.AppliedFilters,
                package.Counts,
                PackageVersion = "2",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "platform_audit_events.json", package.AuditEvents, cancellationToken);
            await WriteCsvEntryAsync(archive, "platform_audit_events.csv", package.AuditEvents, cancellationToken);
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
        PlatformAuditPackageFilter filter,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents.AsNoTracking();
        auditEventsQuery = ApplyAuditEventFilters(auditEventsQuery, filter);

        var tenantsQuery = db.Tenants.AsNoTracking();
        if (filter.TenantId is Guid tenantId)
        {
            tenantsQuery = tenantsQuery.Where(x => x.Id == tenantId);
        }

        var entitlementsQuery = db.Entitlements.AsNoTracking();
        if (filter.TenantId is Guid scopedTenant)
        {
            entitlementsQuery = entitlementsQuery.Where(x => x.TenantId == scopedTenant);
        }

        if (!string.IsNullOrWhiteSpace(filter.ProductKey))
        {
            var productKey = filter.ProductKey.Trim().ToLowerInvariant();
            entitlementsQuery = entitlementsQuery.Where(x => x.ProductKey == productKey);
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

        var serviceClientsQuery = db.ServiceClients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.ProductKey))
        {
            var productKey = filter.ProductKey.Trim().ToLowerInvariant();
            serviceClientsQuery = serviceClientsQuery.Where(x =>
                x.SourceProductKey == productKey
                || x.AllowedProductKeys.Contains(productKey));
        }

        var serviceClients = await serviceClientsQuery
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
        if (filter.TenantId is Guid tokenTenantId)
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
        if (filter.TenantId is Guid allowlistTenantId)
        {
            allowlistQuery = allowlistQuery.Where(x => x.TenantId == allowlistTenantId || x.TenantId == null);
        }

        if (!string.IsNullOrWhiteSpace(filter.ProductKey))
        {
            var productKey = filter.ProductKey.Trim().ToLowerInvariant();
            allowlistQuery = allowlistQuery.Where(x => x.ProductKey == productKey);
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
            filter.TenantId,
            DateTimeOffset.UtcNow,
            new PlatformAuditPackageDateRangeResponse(filter.From, filter.To),
            MapAppliedFilters(filter),
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

    private static IQueryable<PlatformAuditEvent> ApplyAuditEventFilters(
        IQueryable<PlatformAuditEvent> query,
        PlatformAuditPackageFilter filter)
    {
        query = ApplyTenantScope(query, filter.TenantId);
        query = ApplyOccurredAtFilter(query, filter.From, filter.To);

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var action = filter.Action.Trim().ToLowerInvariant();
            query = query.Where(x => x.Action.ToLower() == action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Result))
        {
            var result = filter.Result.Trim().ToLowerInvariant();
            query = query.Where(x => x.Result.ToLower() == result);
        }

        if (!string.IsNullOrWhiteSpace(filter.TargetType))
        {
            var targetType = filter.TargetType.Trim().ToLowerInvariant();
            query = query.Where(x => x.TargetType.ToLower() == targetType);
        }

        if (filter.ActorUserId is Guid actorUserId)
        {
            query = query.Where(x => x.ActorUserId == actorUserId);
        }

        return query;
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

    private static async Task WriteCsvEntryAsync(
        ZipArchive archive,
        string entryName,
        IReadOnlyList<PlatformAuditEventExportItem> events,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        var bytes = PlatformAuditPackageCsvWriter.WriteAuditEvents(events);
        await entryStream.WriteAsync(bytes, cancellationToken);
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

    private static PlatformAuditPackageAppliedFiltersResponse MapAppliedFilters(PlatformAuditPackageFilter filter) =>
        new(
            filter.TenantId,
            filter.From,
            filter.To,
            filter.Action,
            filter.Result,
            filter.TargetType,
            filter.ActorUserId,
            filter.ProductKey);

    private static string BuildFilterReasonCode(PlatformAuditPackageFilter filter) =>
        $"{filter.TenantId}:{filter.Action}:{filter.Result}:{filter.TargetType}:{filter.From:O}|{filter.To:O}";
}
