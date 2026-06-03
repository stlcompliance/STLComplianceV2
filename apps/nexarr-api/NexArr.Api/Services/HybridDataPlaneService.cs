using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Health;
using STLCompliance.Shared.Http;

namespace NexArr.Api.Services;

public sealed class HybridDataPlaneService(
    NexArrDbContext db,
    IHttpClientFactory httpClientFactory,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public const string HttpClientName = "HybridDataPlaneProbe";

    public async Task<PagedResult<DataPlaneProfileResponse>> ListAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        string? productKey,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.DataPlaneProfiles.AsNoTracking()
            .Join(db.Tenants.AsNoTracking(), p => p.TenantId, t => t.Id, (p, t) => new { p, t })
            .Join(db.ProductCatalog.AsNoTracking(), x => x.p.ProductKey, c => c.ProductKey, (x, c) => new { x.p, x.t, c });

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.p.TenantId == scopedTenantId);
        }

        if (!string.IsNullOrWhiteSpace(productKey))
        {
            var normalizedProductKey = productKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.p.ProductKey == normalizedProductKey);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.t.DisplayName)
            .ThenBy(x => x.c.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapResponse(x.p, x.t.Slug, x.t.DisplayName, x.c.DisplayName))
            .ToListAsync(cancellationToken);

        return new PagedResult<DataPlaneProfileResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<IReadOnlyList<DataPlaneDefaultProfileResponse>> ListEffectiveAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var tenantExists = await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        var overrides = await db.DataPlaneProfiles.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .ToDictionaryAsync(p => p.ProductKey, p => p, cancellationToken);

        var products = await db.ProductCatalog.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return products.Select(product =>
        {
            if (overrides.TryGetValue(product.ProductKey, out var profile))
            {
                return new DataPlaneDefaultProfileResponse(
                    tenantId,
                    product.ProductKey,
                    product.DisplayName,
                    profile.DeploymentMode,
                    profile.TrustStatus);
            }

            return new DataPlaneDefaultProfileResponse(
                tenantId,
                product.ProductKey,
                product.DisplayName,
                DataPlaneDeploymentModes.Hosted,
                DataPlaneTrustStatuses.Trusted);
        }).ToList();
    }

    public async Task<DataPlaneProfileResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertDataPlaneProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var deploymentMode = NormalizeDeploymentMode(request.DeploymentMode);
        var trustStatus = NormalizeTrustStatus(request.TrustStatus, deploymentMode);
        var endpointUrl = NormalizeEndpointUrl(request.DataEndpointUrl, deploymentMode);
        return await PersistAsync(
            request.TenantId,
            request.ProductKey,
            deploymentMode,
            endpointUrl,
            trustStatus,
            request.Notes,
            actorUserId,
            cancellationToken);
    }

    public async Task<ValidateDataPlaneProfileResponse> ValidateAsync(
        ClaimsPrincipal principal,
        ValidateDataPlaneProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var normalizedProductKey = request.ProductKey.Trim().ToLowerInvariant();
        var deploymentMode = NormalizeDeploymentMode(request.DeploymentMode);
        var endpointUrl = NormalizeEndpointUrl(request.DataEndpointUrl, deploymentMode);
        var now = DateTimeOffset.UtcNow;

        var (tenantSlug, tenantDisplayName, productDisplayName) = await ResolveTenantAndProductAsync(
            request.TenantId,
            normalizedProductKey,
            cancellationToken);

        var validation = await ProbeAsync(endpointUrl, deploymentMode, cancellationToken);
        var trustStatus = validation.IsTrusted ? DataPlaneTrustStatuses.Trusted : DataPlaneTrustStatuses.PendingValidation;

        var entity = await UpsertProfileAsync(
            request.TenantId,
            normalizedProductKey,
            deploymentMode,
            endpointUrl,
            trustStatus,
            request.Notes,
            actorUserId,
            now,
            cancellationToken);

        await audit.WriteAsync(
            "data_plane.validate",
            "data_plane_profile",
            entity.Id.ToString(),
            validation.IsTrusted ? "Success" : "PendingValidation",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            reasonCode: validation.ErrorCode,
            cancellationToken: cancellationToken);

        return new ValidateDataPlaneProfileResponse(
            MapResponse(entity, tenantSlug, tenantDisplayName, productDisplayName),
            validation.IsTrusted ? "Trusted" : "PendingValidation",
            validation.ReadyUrl,
            validation.LatencyMs,
            validation.ErrorCode,
            validation.ErrorMessage,
            now);
    }

    public async Task DeleteAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string productKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var normalizedProductKey = productKey.Trim().ToLowerInvariant();
        var entity = await db.DataPlaneProfiles
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.ProductKey == normalizedProductKey, cancellationToken);

        if (entity is null)
        {
            return;
        }

        db.DataPlaneProfiles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "data_plane.delete",
            "data_plane_profile",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);
    }

    private async Task<DataPlaneProfileResponse> PersistAsync(
        Guid tenantId,
        string rawProductKey,
        string deploymentMode,
        string? dataEndpointUrl,
        string trustStatus,
        string? notes,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var productKey = rawProductKey.Trim().ToLowerInvariant();
        var (tenantSlug, tenantDisplayName, productDisplayName) = await ResolveTenantAndProductAsync(
            tenantId,
            productKey,
            cancellationToken);

        var entity = await UpsertProfileAsync(
            tenantId,
            productKey,
            deploymentMode,
            dataEndpointUrl,
            trustStatus,
            notes,
            actorUserId,
            now,
            cancellationToken);

        await audit.WriteAsync(
            "data_plane.upsert",
            "data_plane_profile",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity, tenantSlug, tenantDisplayName, productDisplayName);
    }

    private async Task<TenantProductDataPlaneProfile> UpsertProfileAsync(
        Guid tenantId,
        string productKey,
        string deploymentMode,
        string? endpointUrl,
        string trustStatus,
        string? notes,
        Guid? actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var entity = await db.DataPlaneProfiles
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.ProductKey == productKey, cancellationToken);

        if (entity is null)
        {
            entity = new TenantProductDataPlaneProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = productKey,
                CreatedAt = now,
            };
            db.DataPlaneProfiles.Add(entity);
        }

        entity.DeploymentMode = deploymentMode;
        entity.DataEndpointUrl = endpointUrl;
        entity.TrustStatus = trustStatus;
        entity.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        entity.ModifiedByUserId = actorUserId;
        entity.ModifiedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private async Task<(string TenantSlug, string TenantDisplayName, string ProductDisplayName)> ResolveTenantAndProductAsync(
        Guid tenantId,
        string productKey,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        var product = await db.ProductCatalog.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == productKey && p.IsActive, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Active product was not found.", 404);

        return (tenant.Slug, tenant.DisplayName, product.DisplayName);
    }

    private async Task<DataPlaneValidationProbeResult> ProbeAsync(
        string? endpointUrl,
        string deploymentMode,
        CancellationToken cancellationToken)
    {
        if (deploymentMode == DataPlaneDeploymentModes.Hosted)
        {
            return new DataPlaneValidationProbeResult(
                true,
                null,
                null,
                null,
                null,
                null);
        }

        var baseUrl = StlServiceUrl.NormalizeHttpBaseUrl(endpointUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "data_plane.endpoint_required",
                "Data endpoint URL is required for customer-hosted or hybrid deployment modes.",
                400);
        }

        var readyUrl = $"{baseUrl.TrimEnd('/')}/health/ready";
        var client = httpClientFactory.CreateClient(HttpClientName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await client.GetAsync(readyUrl, cancellationToken);
            stopwatch.Stop();
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new DataPlaneValidationProbeResult(
                    false,
                    readyUrl,
                    stopwatch.Elapsed.TotalMilliseconds,
                    $"upstream_{(int)response.StatusCode}",
                    string.IsNullOrWhiteSpace(body) ? "Ready probe returned a non-success status." : body,
                    null);
            }

            var detail = await response.Content.ReadFromJsonAsync<HealthResponse>(cancellationToken);
            var downstreamStatus = detail?.Status ?? "Unknown";
            var isHealthy = downstreamStatus.Equals("Healthy", StringComparison.OrdinalIgnoreCase);
            return new DataPlaneValidationProbeResult(
                isHealthy,
                readyUrl,
                stopwatch.Elapsed.TotalMilliseconds,
                isHealthy ? null : "downstream_not_healthy",
                isHealthy ? null : $"Downstream reported status '{downstreamStatus}'.",
                detail);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return new DataPlaneValidationProbeResult(
                false,
                readyUrl,
                stopwatch.Elapsed.TotalMilliseconds,
                "probe_timeout",
                "Ready probe timed out.",
                null);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return new DataPlaneValidationProbeResult(
                false,
                readyUrl,
                stopwatch.Elapsed.TotalMilliseconds,
                "upstream_unreachable",
                ex.Message,
                null);
        }
    }

    private static DataPlaneProfileResponse MapResponse(
        TenantProductDataPlaneProfile profile,
        string tenantSlug,
        string tenantDisplayName,
        string productDisplayName) =>
        new(
            profile.Id,
            profile.TenantId,
            tenantSlug,
            tenantDisplayName,
            profile.ProductKey,
            productDisplayName,
            profile.DeploymentMode,
            profile.DataEndpointUrl,
            profile.TrustStatus,
            profile.Notes,
            profile.ModifiedAt);

    private static string NormalizeDeploymentMode(string raw)
    {
        var mode = raw.Trim().ToLowerInvariant();
        if (!DataPlaneDeploymentModes.All.Contains(mode))
        {
            throw new StlApiException(
                "data_plane.invalid_mode",
                $"Deployment mode must be one of: {string.Join(", ", DataPlaneDeploymentModes.All)}.",
                400);
        }

        return mode;
    }

    private static string NormalizeTrustStatus(string raw, string deploymentMode)
    {
        var status = raw.Trim().ToLowerInvariant();
        if (!DataPlaneTrustStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "data_plane.invalid_trust_status",
                $"Trust status must be one of: {string.Join(", ", DataPlaneTrustStatuses.All)}.",
                400);
        }

        if (deploymentMode == DataPlaneDeploymentModes.CustomerHosted
            && status == DataPlaneTrustStatuses.Trusted)
        {
            throw new StlApiException(
                "data_plane.customer_hosted_untrusted",
                "Customer-hosted data planes must remain untrusted or pending validation until the owning service validates them.",
                400);
        }

        return status;
    }

    private static string? NormalizeEndpointUrl(string? raw, string deploymentMode)
    {
        if (deploymentMode == DataPlaneDeploymentModes.Hosted)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new StlApiException(
                "data_plane.endpoint_required",
                "Data endpoint URL is required for customer-hosted or hybrid deployment modes.",
                400);
        }

        var trimmed = raw.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new StlApiException(
                "data_plane.invalid_endpoint",
                "Data endpoint URL must be an absolute http or https URL.",
                400);
        }

        return trimmed;
    }

    private sealed record DataPlaneValidationProbeResult(
        bool IsTrusted,
        string? ReadyUrl,
        double? LatencyMs,
        string? ErrorCode,
        string? ErrorMessage,
        HealthResponse? Detail);
}
