using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class TenantIntegrationService(
    NexArrDbContext db,
    TenantIntegrationCredentialProtector credentialProtector,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outbox,
    IHttpClientFactory httpClientFactory,
    IOptions<TenantIntegrationOptions> options)
{
    public const string HttpClientName = "TenantIntegrationConnector";
    public const string ProcessSyncActionScope = "nexarr.integrations.sync";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000c1");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<TenantIntegrationCatalogResponse> GetCatalogAsync() =>
        Task.FromResult(TenantIntegrationProviderCatalog.BuildResponse());

    public async Task<PagedResult<TenantIntegrationConnectionResponse>> ListAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        string? providerKey,
        int page,
        int pageSize,
        bool platformScope,
        CancellationToken cancellationToken = default)
    {
        if (platformScope)
        {
            await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        }
        else
        {
            if (tenantId is not Guid requiredTenantId)
            {
                requiredTenantId = principal.GetTenantId();
            }

            await authorization.RequireTenantAccessAsync(principal, requiredTenantId, allowTenantAdmin: true, cancellationToken);
            tenantId = requiredTenantId;
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.TenantIntegrationConnections
            .AsNoTracking()
            .Join(db.Tenants.AsNoTracking(), c => c.TenantId, t => t.Id, (c, t) => new { c, t });

        if (tenantId is Guid scopedTenant)
        {
            query = query.Where(x => x.c.TenantId == scopedTenant);
        }

        if (!string.IsNullOrWhiteSpace(providerKey))
        {
            var normalized = TenantIntegrationProviderCatalog.NormalizeProviderKey(providerKey);
            query = query.Where(x => x.c.ProviderKey == normalized);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.t.DisplayName)
            .ThenBy(x => x.c.ProviderKey)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<TenantIntegrationConnectionResponse>(rows.Count);
        foreach (var row in rows)
        {
            items.Add(await MapConnectionAsync(row.c, row.t, cancellationToken));
        }

        return new PagedResult<TenantIntegrationConnectionResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    public async Task<TenantIntegrationConnectionResponse> GetAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            providerKey,
            actorUserId: principal.GetUserId(),
            saveWhenCreated: false,
            cancellationToken);
        var tenant = await GetTenantAsync(tenantId, cancellationToken);
        return await MapConnectionAsync(connection, tenant, cancellationToken);
    }

    public async Task<TenantIntegrationConnectionResponse> UpsertAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        UpsertTenantIntegrationConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            definition.ProviderKey,
            actorUserId,
            saveWhenCreated: true,
            cancellationToken);

        var syncDirection = NormalizeDirection(request.SyncDirection ?? connection.SyncDirection);
        if (request.WritebacksEnabled == true && !definition.SupportsWriteback)
        {
            throw new StlApiException(
                "integrations.writeback_unsupported",
                $"{definition.DisplayName} does not support writeback in NexArr.",
                400);
        }

        if (request.WritebacksEnabled == true
            && syncDirection is TenantIntegrationDirections.ReadOnly or TenantIntegrationDirections.Inbound)
        {
            throw new StlApiException(
                "integrations.writeback_direction_required",
                "Writebacks require outbound, bidirectional, or writeback sync direction.",
                400);
        }

        var status = NormalizeConnectionStatus(request.Status ?? connection.Status);
        var configJson = NormalizeJsonObject(request.ConfigurationJson ?? connection.ConfigurationJson);
        var now = DateTimeOffset.UtcNow;
        connection.Status = status;
        connection.SyncDirection = syncDirection;
        connection.WritebacksEnabled = request.WritebacksEnabled ?? connection.WritebacksEnabled;
        connection.ManualMappingRequired = request.ManualMappingRequired ?? definition.RequiresManualMapping;
        connection.ConfigurationJson = configJson;
        connection.ModifiedByUserId = actorUserId;
        connection.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant_integrations.connection_upserted",
            "tenant_integration_connection",
            connection.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            reasonCode: definition.ProviderKey,
            cancellationToken: cancellationToken);

        return await MapConnectionAsync(connection, await GetTenantAsync(tenantId, cancellationToken), cancellationToken);
    }

    public async Task<TenantIntegrationConnectionResponse> UpsertCredentialAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        UpsertTenantIntegrationCredentialRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            definition.ProviderKey,
            actorUserId,
            saveWhenCreated: true,
            cancellationToken);

        if (request.Payload.Count == 0)
        {
            throw new StlApiException(
                "integrations.credentials_empty",
                "Credential payload must contain at least one secret field.",
                400);
        }

        var plaintext = JsonSerializer.Serialize(request.Payload, JsonOptions);
        var protectedSecret = credentialProtector.Protect(plaintext);
        var now = DateTimeOffset.UtcNow;
        var credential = await db.TenantIntegrationCredentials
            .FirstOrDefaultAsync(x => x.ConnectionId == connection.Id, cancellationToken);

        if (credential is null)
        {
            credential = new TenantIntegrationCredential
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConnectionId = connection.Id,
                ProviderKey = definition.ProviderKey,
                CreatedByUserId = actorUserId,
                CreatedAt = now,
            };
            db.TenantIntegrationCredentials.Add(credential);
        }

        credential.CredentialKind = NormalizeCredentialKind(request.CredentialKind, definition.AuthType);
        credential.EncryptedPayload = protectedSecret.CipherText;
        credential.EncryptionKeyId = protectedSecret.KeyId;
        credential.RedactedLabel = BuildRedactedLabel(request.SecretLabel, request.Payload);
        credential.ExpiresAt = request.ExpiresAt;
        credential.LastValidatedAt = null;
        credential.ModifiedByUserId = actorUserId;
        credential.UpdatedAt = now;

        connection.Status = TenantIntegrationStatuses.Configured;
        connection.ModifiedByUserId = actorUserId;
        connection.UpdatedAt = now;
        connection.LastErrorCategory = null;
        connection.LastErrorMessage = null;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant_integrations.credentials_rotated",
            "tenant_integration_credential",
            credential.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            reasonCode: definition.ProviderKey,
            cancellationToken: cancellationToken);

        return await MapConnectionAsync(connection, await GetTenantAsync(tenantId, cancellationToken), cancellationToken);
    }

    public async Task DeleteCredentialAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var connection = await LoadConnectionAsync(tenantId, definition.ProviderKey, cancellationToken);
        if (connection is null)
        {
            return;
        }

        var credentials = await db.TenantIntegrationCredentials
            .Where(x => x.ConnectionId == connection.Id)
            .ToListAsync(cancellationToken);

        if (credentials.Count == 0)
        {
            return;
        }

        db.TenantIntegrationCredentials.RemoveRange(credentials);
        connection.Status = TenantIntegrationStatuses.NotConfigured;
        connection.ModifiedByUserId = actorUserId;
        connection.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant_integrations.credentials_deleted",
            "tenant_integration_connection",
            connection.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            reasonCode: definition.ProviderKey,
            cancellationToken: cancellationToken);
    }

    public async Task<TestTenantIntegrationConnectionResponse> TestAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            definition.ProviderKey,
            actorUserId,
            saveWhenCreated: true,
            cancellationToken);

        var result = await ProbeAsync(connection, definition, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        connection.Status = result.Status == "healthy"
            ? TenantIntegrationStatuses.Connected
            : TenantIntegrationStatuses.Degraded;
        connection.LastErrorCategory = result.ErrorCategory;
        connection.LastErrorMessage = result.ErrorMessage;
        connection.ModifiedByUserId = actorUserId;
        connection.UpdatedAt = now;

        db.TenantIntegrationProviderHealth.Add(new TenantIntegrationProviderHealth
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConnectionId = connection.Id,
            ProviderKey = definition.ProviderKey,
            Status = result.Status,
            CheckedAt = now,
            LatencyMs = result.LatencyMs,
            ErrorCategory = result.ErrorCategory,
            ErrorMessage = result.ErrorMessage,
            MetadataJson = result.MetadataJson,
        });

        var credential = await db.TenantIntegrationCredentials
            .FirstOrDefaultAsync(x => x.ConnectionId == connection.Id, cancellationToken);
        if (credential is not null)
        {
            credential.LastValidatedAt = result.Status == "healthy" ? now : credential.LastValidatedAt;
            credential.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "tenant_integrations.connection_tested",
            "tenant_integration_connection",
            connection.Id.ToString(),
            result.Status == "healthy" ? "Success" : "Failure",
            tenantId: tenantId,
            actorUserId: actorUserId,
            reasonCode: result.ErrorCategory ?? definition.ProviderKey,
            cancellationToken: cancellationToken);

        return new TestTenantIntegrationConnectionResponse(
            connection.Id,
            definition.ProviderKey,
            result.Status,
            result.ErrorCategory,
            result.ErrorMessage,
            result.LatencyMs,
            now);
    }

    public async Task<TenantIntegrationSyncRunResponse> CreateSyncRunAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        TriggerTenantIntegrationSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            definition.ProviderKey,
            actorUserId,
            saveWhenCreated: true,
            cancellationToken);

        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? $"manual:{DateTimeOffset.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid():N}"
            : request.IdempotencyKey.Trim();

        var run = await CreateSyncRunRecordAsync(
            connection,
            TenantIntegrationTriggerKinds.Manual,
            actorUserId,
            idempotencyKey,
            cancellationToken);

        await ExecuteSyncRunAsync(run.Id, cancellationToken);

        var updated = await db.TenantIntegrationSyncRuns.AsNoTracking()
            .FirstAsync(x => x.Id == run.Id, cancellationToken);
        await audit.WriteAsync(
            "tenant_integrations.sync_triggered",
            "tenant_integration_sync_run",
            run.Id.ToString(),
            updated.Status == TenantIntegrationSyncRunStatuses.Succeeded ? "Success" : "Failure",
            tenantId: tenantId,
            actorUserId: actorUserId,
            reasonCode: updated.Status,
            cancellationToken: cancellationToken);

        return MapSyncRun(updated);
    }

    public async Task<IReadOnlyList<TenantIntegrationSyncRunResponse>> ListSyncRunsAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var definition = ResolveProvider(providerKey);
        var take = Math.Clamp(limit, 1, 50);
        return await db.TenantIntegrationSyncRuns.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProviderKey == definition.ProviderKey)
            .OrderByDescending(x => x.StartedAt)
            .Take(take)
            .Select(x => MapSyncRun(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcessTenantIntegrationSyncResponse> ProcessBatchAsync(
        ProcessTenantIntegrationSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = Math.Clamp(request.BatchSize ?? options.Value.WorkerBatchSize, 1, 100);
        await EnsureScheduledRunsAsync(asOf, batchSize, cancellationToken);

        var candidates = await db.TenantIntegrationSyncRuns
            .Where(x => (x.Status == TenantIntegrationSyncRunStatuses.Queued
                    || x.Status == TenantIntegrationSyncRunStatuses.Failed
                    || x.Status == TenantIntegrationSyncRunStatuses.SourceUnavailable)
                && (x.NextRetryAt == null || x.NextRetryAt <= asOf))
            .OrderBy(x => x.StartedAt)
            .Take(batchSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var processed = new List<Guid>();
        var succeeded = 0;
        var failed = 0;
        var needsReview = 0;
        var sourceUnavailable = 0;
        var deadLetter = 0;

        foreach (var runId in candidates)
        {
            await ExecuteSyncRunAsync(runId, cancellationToken);
            var run = await db.TenantIntegrationSyncRuns.AsNoTracking()
                .FirstAsync(x => x.Id == runId, cancellationToken);
            processed.Add(runId);
            succeeded += run.Status == TenantIntegrationSyncRunStatuses.Succeeded ? 1 : 0;
            failed += run.Status == TenantIntegrationSyncRunStatuses.Failed ? 1 : 0;
            needsReview += run.Status == TenantIntegrationSyncRunStatuses.NeedsReview ? 1 : 0;
            sourceUnavailable += run.Status == TenantIntegrationSyncRunStatuses.SourceUnavailable ? 1 : 0;
            deadLetter += run.Status == TenantIntegrationSyncRunStatuses.DeadLetter ? 1 : 0;
        }

        return new ProcessTenantIntegrationSyncResponse(
            asOf,
            batchSize,
            candidates.Count,
            succeeded,
            failed,
            needsReview,
            sourceUnavailable,
            deadLetter,
            0,
            processed);
    }

    public async Task<IReadOnlyList<TenantIntegrationMappingTemplateResponse>> ListMappingTemplatesAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var definition = ResolveProvider(providerKey);
        return await db.TenantIntegrationManualMappingTemplates.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProviderKey == definition.ProviderKey)
            .OrderBy(x => x.TemplateName)
            .Select(x => MapTemplate(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantIntegrationMappingTemplateResponse> UpsertMappingTemplateAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        UpsertTenantIntegrationMappingTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var targetProduct = request.TargetProductKey.Trim().ToLowerInvariant();
        if (!definition.OwningProducts.Contains(targetProduct, StringComparer.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "integrations.mapping_target_product_invalid",
                $"{definition.DisplayName} mappings must route to one of: {string.Join(", ", definition.OwningProducts)}.",
                400);
        }

        var mappingJson = NormalizeJsonObject(request.MappingJson);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            definition.ProviderKey,
            actorUserId,
            saveWhenCreated: true,
            cancellationToken);

        var templateName = request.TemplateName.Trim();
        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new StlApiException("integrations.mapping_name_required", "Mapping template name is required.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        var template = await db.TenantIntegrationManualMappingTemplates
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ProviderKey == definition.ProviderKey
                    && x.TemplateName == templateName,
                cancellationToken);

        if (template is null)
        {
            template = new TenantIntegrationManualMappingTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConnectionId = connection.Id,
                ProviderKey = definition.ProviderKey,
                TemplateName = templateName,
                CreatedByUserId = actorUserId,
                CreatedAt = now,
            };
            db.TenantIntegrationManualMappingTemplates.Add(template);
        }

        template.SourceEntityType = request.SourceEntityType.Trim();
        template.TargetProductKey = targetProduct;
        template.TargetEntityType = request.TargetEntityType.Trim();
        template.MappingJson = mappingJson;
        template.IsActive = request.IsActive;
        template.ModifiedByUserId = actorUserId;
        template.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant_integrations.mapping_template_upserted",
            "tenant_integration_mapping_template",
            template.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            reasonCode: definition.ProviderKey,
            cancellationToken: cancellationToken);

        return MapTemplate(template);
    }

    public async Task DeleteMappingTemplateAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        Guid mappingTemplateId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var definition = ResolveProvider(providerKey);
        var template = await db.TenantIntegrationManualMappingTemplates
            .FirstOrDefaultAsync(
                x => x.Id == mappingTemplateId
                    && x.TenantId == tenantId
                    && x.ProviderKey == definition.ProviderKey,
                cancellationToken);
        if (template is null)
        {
            return;
        }

        db.TenantIntegrationManualMappingTemplates.Remove(template);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "tenant_integrations.mapping_template_deleted",
            "tenant_integration_mapping_template",
            template.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: principal.GetUserId(),
            reasonCode: definition.ProviderKey,
            cancellationToken: cancellationToken);
    }

    public async Task<TenantIntegrationExternalMappingResponse> UpsertExternalMappingAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        UpsertTenantIntegrationExternalMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var actorUserId = principal.GetUserId();
        var definition = ResolveProvider(providerKey);
        var connection = await LoadOrCreateConnectionAsync(
            tenantId,
            definition.ProviderKey,
            actorUserId,
            saveWhenCreated: true,
            cancellationToken);

        var externalEntityType = request.ExternalEntityType.Trim();
        var externalId = request.ExternalId.Trim();
        var now = DateTimeOffset.UtcNow;
        var mapping = await db.TenantIntegrationExternalMappings
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ProviderKey == definition.ProviderKey
                    && x.ExternalEntityType == externalEntityType
                    && x.ExternalId == externalId,
                cancellationToken);
        if (mapping is null)
        {
            mapping = new TenantIntegrationExternalMapping
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConnectionId = connection.Id,
                ProviderKey = definition.ProviderKey,
                ExternalEntityType = externalEntityType,
                ExternalId = externalId,
                CreatedAt = now,
            };
            db.TenantIntegrationExternalMappings.Add(mapping);
        }

        mapping.OwningProductKey = request.OwningProductKey.Trim().ToLowerInvariant();
        mapping.StlEntityType = request.StlEntityType.Trim();
        mapping.StlEntityId = request.StlEntityId.Trim();
        mapping.MappingStatus = request.MappingStatus.Trim().ToLowerInvariant();
        mapping.SyncDirection = NormalizeDirection(request.SyncDirection);
        mapping.LastVerifiedAt = now;
        mapping.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return MapExternalMapping(mapping);
    }

    public async Task<IReadOnlyList<TenantIntegrationExternalMappingResponse>> ListExternalMappingsAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
        var definition = ResolveProvider(providerKey);
        return await db.TenantIntegrationExternalMappings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProviderKey == definition.ProviderKey)
            .OrderBy(x => x.ExternalEntityType)
            .ThenBy(x => x.ExternalId)
            .Select(x => MapExternalMapping(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantIntegrationIntakeAttemptResponse> RecordCallbackAsync(
        string providerKey,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.Request.Query["state"].ToString();
        var statePayload = ParseCallbackState(state);
        var definition = ResolveProvider(providerKey);
        if (!string.Equals(statePayload.ProviderKey, definition.ProviderKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("integrations.oauth_state_provider_mismatch", "OAuth state does not match the callback provider.", 400);
        }

        var connection = await LoadConnectionAsync(statePayload.TenantId, definition.ProviderKey, cancellationToken);
        return await RecordIntakeAsync(
            definition.ProviderKey,
            "oauth_callback",
            context,
            statePayload.TenantId,
            connection?.Id,
            requireSharedSecret: false,
            cancellationToken);
    }

    public async Task<TenantIntegrationIntakeAttemptResponse> RecordExternalIntakeAsync(
        string providerKey,
        string intakeKind,
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var definition = ResolveProvider(providerKey);
        var tenantIdText = context.Request.Query["tenantId"].ToString();
        if (!Guid.TryParse(tenantIdText, out var tenantId))
        {
            throw new StlApiException(
                "integrations.tenant_required",
                "tenantId query parameter is required for external integration intake.",
                400);
        }

        var connection = await LoadConnectionAsync(tenantId, definition.ProviderKey, cancellationToken)
            ?? throw new StlApiException(
                "integrations.connection_not_found",
                "Tenant integration connection was not found.",
                404);

        var requireSecret = intakeKind is "webhook" or "scim" or "as2" or "sftp";
        return await RecordIntakeAsync(
            definition.ProviderKey,
            intakeKind,
            context,
            tenantId,
            connection.Id,
            requireSecret,
            cancellationToken);
    }

    public string BuildSamlMetadata(string providerKey)
    {
        var definition = ResolveProvider(providerKey);
        var entityId = $"urn:stl:nexarr:integrations:{definition.ProviderKey}";
        var acsPath = $"/api/v1/integrations/{definition.ProviderKey}/saml/acs";
        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <EntityDescriptor entityID="{WebUtility.HtmlEncode(entityId)}" xmlns="urn:oasis:names:tc:SAML:2.0:metadata">
              <SPSSODescriptor protocolSupportEnumeration="urn:oasis:names:tc:SAML:2.0:protocol">
                <AssertionConsumerService Binding="urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST" Location="{WebUtility.HtmlEncode(acsPath)}" index="1" />
              </SPSSODescriptor>
            </EntityDescriptor>
            """;
    }

    public static string BuildCallbackState(Guid tenantId, string providerKey) =>
        Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new CallbackState(
            tenantId,
            TenantIntegrationProviderCatalog.NormalizeProviderKey(providerKey),
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow)));

    private async Task<TenantIntegrationIntakeAttemptResponse> RecordIntakeAsync(
        string providerKey,
        string intakeKind,
        HttpContext context,
        Guid? tenantId,
        Guid? connectionId,
        bool requireSharedSecret,
        CancellationToken cancellationToken)
    {
        if (requireSharedSecret)
        {
            await ValidateSharedSecretAsync(tenantId, connectionId, context, cancellationToken);
        }

        var body = await ReadRequestBodyAsync(context.Request, cancellationToken);
        var payloadHash = ComputeSha256Hex(body);
        var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            idempotencyKey = context.Request.Headers["X-STL-Idempotency-Key"].ToString();
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            idempotencyKey = $"{providerKey}:{intakeKind}:{payloadHash}";
        }

        var existing = await db.TenantIntegrationIntakeAttempts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProviderKey == providerKey && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return MapIntake(existing);
        }

        var now = DateTimeOffset.UtcNow;
        var attempt = new TenantIntegrationIntakeAttempt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConnectionId = connectionId,
            ProviderKey = providerKey,
            IntakeKind = intakeKind,
            IdempotencyKey = idempotencyKey,
            Status = "received",
            SourceRoute = context.Request.Path.ToString(),
            PayloadHash = payloadHash,
            FileName = context.Request.Headers["Content-Disposition"].ToString(),
            ContentType = context.Request.ContentType,
            ReceivedAt = now,
            ProcessedAt = now,
        };
        db.TenantIntegrationIntakeAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        if (connectionId is Guid scopedConnectionId)
        {
            var connection = await db.TenantIntegrationConnections
                .FirstOrDefaultAsync(x => x.Id == scopedConnectionId, cancellationToken);
            if (connection is not null)
            {
                await CreateSyncRunRecordAsync(
                    connection,
                    intakeKind is "webhook" or "scim" ? TenantIntegrationTriggerKinds.Webhook : TenantIntegrationTriggerKinds.File,
                    null,
                    $"intake:{attempt.Id:N}",
                    cancellationToken);
            }
        }

        return MapIntake(attempt);
    }

    private async Task ExecuteSyncRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await db.TenantIntegrationSyncRuns
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken);
        if (run is null)
        {
            return;
        }

        var connection = await db.TenantIntegrationConnections
            .FirstAsync(x => x.Id == run.ConnectionId, cancellationToken);
        var definition = ResolveProvider(run.ProviderKey);
        var now = DateTimeOffset.UtcNow;
        run.Status = TenantIntegrationSyncRunStatuses.Running;
        run.AttemptCount += 1;
        run.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var activeMappingCount = await db.TenantIntegrationManualMappingTemplates.CountAsync(
            x => x.ConnectionId == connection.Id && x.IsActive,
            cancellationToken);

        if (definition.RequiresManualMapping && activeMappingCount == 0)
        {
            await CompleteRunAsync(
                run,
                connection,
                TenantIntegrationSyncRunStatuses.NeedsReview,
                "manual_mapping_required",
                "This provider requires at least one active manual mapping before sync can route data.",
                snapshotCount: 0,
                mappingCount: 0,
                resultSummary: new
                {
                    definition.ProviderKey,
                    definition.DisplayName,
                    Freshness = "needs_review",
                    DestinationProducts = definition.OwningProducts,
                },
                cancellationToken);
            return;
        }

        var credentialMissing = await CredentialRequiredAndMissingAsync(connection, definition, cancellationToken);
        if (credentialMissing)
        {
            await CompleteRunAsync(
                run,
                connection,
                TenantIntegrationSyncRunStatuses.Failed,
                "credentials_missing",
                "Credentials are required before this provider can sync.",
                snapshotCount: 0,
                mappingCount: activeMappingCount,
                resultSummary: new
                {
                    definition.ProviderKey,
                    definition.DisplayName,
                    Freshness = "unknown",
                    DestinationProducts = definition.OwningProducts,
                },
                cancellationToken);
            return;
        }

        var syncResult = await FetchSnapshotAsync(connection, definition, cancellationToken);
        var finalStatus = syncResult.Status switch
        {
            "source_unavailable" => TenantIntegrationSyncRunStatuses.SourceUnavailable,
            "failed" => TenantIntegrationSyncRunStatuses.Failed,
            _ => TenantIntegrationSyncRunStatuses.Succeeded,
        };

        await CompleteRunAsync(
            run,
            connection,
            finalStatus,
            syncResult.ErrorCategory,
            syncResult.ErrorMessage,
            syncResult.SnapshotCount,
            activeMappingCount,
            syncResult.ResultSummary,
            cancellationToken);

        if (finalStatus == TenantIntegrationSyncRunStatuses.Succeeded)
        {
            await outbox.TryEnqueueAsync(
                "nexarr.integration_snapshot.received",
                "tenant_integration_connection",
                connection.Id.ToString(),
                run.Id.ToString(),
                new PlatformOutboxPayload(
                    SchemaVersion: 1,
                    TenantId: connection.TenantId,
                    ActorPersonId: run.TriggeredByUserId,
                    TargetType: "tenant_integration_connection",
                    TargetId: connection.Id.ToString(),
                    Summary: $"{definition.DisplayName} external snapshot was received for tenant integration routing.",
                    Metadata: new Dictionary<string, string>
                    {
                        ["productCode"] = "nexarr",
                        ["providerKey"] = definition.ProviderKey,
                        ["providerDisplayName"] = definition.DisplayName,
                        ["syncRunId"] = run.Id.ToString(),
                        ["destinationProducts"] = string.Join(",", definition.OwningProducts),
                        ["snapshotCount"] = syncResult.SnapshotCount.ToString(),
                        ["freshness"] = "snapshot",
                        ["source"] = "external_integration",
                    }),
                cancellationToken: cancellationToken);
        }
    }

    private async Task CompleteRunAsync(
        TenantIntegrationSyncRun run,
        TenantIntegrationConnection connection,
        string status,
        string? errorCategory,
        string? errorMessage,
        int snapshotCount,
        int mappingCount,
        object resultSummary,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var maxAttempts = Math.Clamp(options.Value.MaxRetryAttempts, 1, 25);
        var terminalStatus = status;
        if (status is TenantIntegrationSyncRunStatuses.Failed or TenantIntegrationSyncRunStatuses.SourceUnavailable
            && run.AttemptCount >= maxAttempts)
        {
            terminalStatus = TenantIntegrationSyncRunStatuses.DeadLetter;
        }

        run.Status = terminalStatus;
        run.CompletedAt = now;
        run.NextRetryAt = terminalStatus is TenantIntegrationSyncRunStatuses.Failed or TenantIntegrationSyncRunStatuses.SourceUnavailable
            ? now.AddMinutes(Math.Clamp(options.Value.RetryIntervalMinutes, 1, 24 * 60))
            : null;
        run.SnapshotCount = snapshotCount;
        run.MappingCount = mappingCount;
        run.ErrorCategory = errorCategory;
        run.ErrorMessage = Truncate(errorMessage, 1024);
        run.DestinationProductsJson = JsonSerializer.Serialize(
            ResolveProvider(run.ProviderKey).OwningProducts,
            JsonOptions);
        run.ResultSummaryJson = JsonSerializer.Serialize(resultSummary, JsonOptions);
        run.UpdatedAt = now;

        if (terminalStatus == TenantIntegrationSyncRunStatuses.Succeeded)
        {
            connection.LastSuccessfulSyncAt = now;
            connection.LastErrorCategory = null;
            connection.LastErrorMessage = null;
            if (connection.Status != TenantIntegrationStatuses.Disabled)
            {
                connection.Status = TenantIntegrationStatuses.Connected;
            }
        }
        else if (terminalStatus == TenantIntegrationSyncRunStatuses.NeedsReview)
        {
            connection.Status = TenantIntegrationStatuses.NeedsReview;
            connection.LastErrorCategory = errorCategory;
            connection.LastErrorMessage = Truncate(errorMessage, 1024);
        }
        else
        {
            connection.LastFailedSyncAt = now;
            connection.LastErrorCategory = errorCategory;
            connection.LastErrorMessage = Truncate(errorMessage, 1024);
            if (connection.Status != TenantIntegrationStatuses.Disabled)
            {
                connection.Status = TenantIntegrationStatuses.Degraded;
            }
        }

        connection.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ConnectorSyncResult> FetchSnapshotAsync(
        TenantIntegrationConnection connection,
        TenantIntegrationProviderDefinition definition,
        CancellationToken cancellationToken)
    {
        var config = ParseConfiguration(connection.ConfigurationJson);
        var targetUrl = ResolveString(config, "syncUrl", "healthUrl", "baseUrl");
        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            return new ConnectorSyncResult(
                "succeeded",
                1,
                null,
                null,
                new
                {
                    definition.ProviderKey,
                    definition.DisplayName,
                    SnapshotAt = DateTimeOffset.UtcNow,
                    Freshness = "snapshot",
                    SourceMode = "configured_connector",
                    DestinationProducts = definition.OwningProducts,
                    Note = "No provider syncUrl was configured; NexArr recorded a configuration snapshot only.",
                });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);
        await ApplyCredentialHeadersAsync(request, connection.Id, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var response = await httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
            stopwatch.Stop();
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode >= 500 ? "source_unavailable" : "failed";
                return new ConnectorSyncResult(
                    status,
                    0,
                    $"upstream_{(int)response.StatusCode}",
                    Truncate(body, 1024),
                    new
                    {
                        definition.ProviderKey,
                        StatusCode = (int)response.StatusCode,
                        LatencyMs = stopwatch.Elapsed.TotalMilliseconds,
                    });
            }

            return new ConnectorSyncResult(
                "succeeded",
                1,
                null,
                null,
                new
                {
                    definition.ProviderKey,
                    definition.DisplayName,
                    SnapshotAt = DateTimeOffset.UtcNow,
                    Freshness = "snapshot",
                    DestinationProducts = definition.OwningProducts,
                    PayloadHash = ComputeSha256Hex(body),
                    LatencyMs = stopwatch.Elapsed.TotalMilliseconds,
                });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ConnectorSyncResult("source_unavailable", 0, "timeout", "Provider sync timed out.", new { definition.ProviderKey });
        }
        catch (HttpRequestException ex)
        {
            return new ConnectorSyncResult("source_unavailable", 0, "source_unavailable", ex.Message, new { definition.ProviderKey });
        }
    }

    private async Task<ConnectorProbeResult> ProbeAsync(
        TenantIntegrationConnection connection,
        TenantIntegrationProviderDefinition definition,
        CancellationToken cancellationToken)
    {
        if (await CredentialRequiredAndMissingAsync(connection, definition, cancellationToken))
        {
            return new ConnectorProbeResult(
                "degraded",
                null,
                "credentials_missing",
                "Credentials are required before this provider can connect.",
                "{}");
        }

        var config = ParseConfiguration(connection.ConfigurationJson);
        var healthUrl = ResolveString(config, "healthUrl", "baseUrl");
        if (string.IsNullOrWhiteSpace(healthUrl))
        {
            return new ConnectorProbeResult(
                "healthy",
                null,
                null,
                null,
                JsonSerializer.Serialize(new
                {
                    definition.ProviderKey,
                    SourceMode = "configuration",
                    Message = "Credentials and configuration are present; no healthUrl was configured."
                }, JsonOptions));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, healthUrl);
        await ApplyCredentialHeadersAsync(request, connection.Id, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var response = await httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
            stopwatch.Stop();
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ConnectorProbeResult(
                    "degraded",
                    stopwatch.Elapsed.TotalMilliseconds,
                    $"upstream_{(int)response.StatusCode}",
                    string.IsNullOrWhiteSpace(body) ? "Provider health check returned a non-success status." : Truncate(body, 1024),
                    "{}");
            }

            return new ConnectorProbeResult(
                "healthy",
                stopwatch.Elapsed.TotalMilliseconds,
                null,
                null,
                JsonSerializer.Serialize(new
                {
                    StatusCode = (int)response.StatusCode,
                    LatencyMs = stopwatch.Elapsed.TotalMilliseconds
                }, JsonOptions));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return new ConnectorProbeResult("degraded", stopwatch.Elapsed.TotalMilliseconds, "timeout", "Provider health check timed out.", "{}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return new ConnectorProbeResult("degraded", stopwatch.Elapsed.TotalMilliseconds, "source_unavailable", ex.Message, "{}");
        }
    }

    private async Task EnsureScheduledRunsAsync(
        DateTimeOffset asOf,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var dueConnections = await db.TenantIntegrationConnections
            .Where(x => x.Status != TenantIntegrationStatuses.Disabled
                && x.Status != TenantIntegrationStatuses.NotConfigured
                && !db.TenantIntegrationSyncRuns.Any(r =>
                    r.ConnectionId == x.Id
                    && (r.Status == TenantIntegrationSyncRunStatuses.Queued
                        || r.Status == TenantIntegrationSyncRunStatuses.Running)))
            .OrderBy(x => x.LastSuccessfulSyncAt ?? DateTimeOffset.MinValue)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        foreach (var connection in dueConnections)
        {
            var idempotencyKey = $"worker:{asOf:yyyyMMddHHmm}:{connection.Id:N}";
            await CreateSyncRunRecordAsync(
                connection,
                TenantIntegrationTriggerKinds.Worker,
                WorkerActorUserId,
                idempotencyKey,
                cancellationToken);
        }
    }

    private async Task<TenantIntegrationSyncRun> CreateSyncRunRecordAsync(
        TenantIntegrationConnection connection,
        string triggeredBy,
        Guid? actorUserId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var existing = await db.TenantIntegrationSyncRuns
            .FirstOrDefaultAsync(
                x => x.TenantId == connection.TenantId
                    && x.ProviderKey == connection.ProviderKey
                    && x.IdempotencyKey == idempotencyKey,
                cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var definition = ResolveProvider(connection.ProviderKey);
        var now = DateTimeOffset.UtcNow;
        var run = new TenantIntegrationSyncRun
        {
            Id = Guid.NewGuid(),
            TenantId = connection.TenantId,
            ConnectionId = connection.Id,
            ProviderKey = connection.ProviderKey,
            Status = TenantIntegrationSyncRunStatuses.Queued,
            Direction = connection.SyncDirection,
            TriggeredBy = triggeredBy,
            TriggeredByUserId = actorUserId,
            IdempotencyKey = idempotencyKey,
            DestinationProductsJson = JsonSerializer.Serialize(definition.OwningProducts, JsonOptions),
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.TenantIntegrationSyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    private async Task<TenantIntegrationConnection> LoadOrCreateConnectionAsync(
        Guid tenantId,
        string providerKey,
        Guid actorUserId,
        bool saveWhenCreated,
        CancellationToken cancellationToken)
    {
        var definition = ResolveProvider(providerKey);
        var connection = await LoadConnectionAsync(tenantId, definition.ProviderKey, cancellationToken);
        if (connection is not null)
        {
            return connection;
        }

        _ = await GetTenantAsync(tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        connection = new TenantIntegrationConnection
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderKey = definition.ProviderKey,
            Status = TenantIntegrationStatuses.NotConfigured,
            SyncDirection = definition.DefaultDirection,
            WritebacksEnabled = false,
            ManualMappingRequired = definition.RequiresManualMapping,
            ConfigurationJson = "{}",
            CreatedByUserId = actorUserId,
            ModifiedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (saveWhenCreated)
        {
            db.TenantIntegrationConnections.Add(connection);
            await db.SaveChangesAsync(cancellationToken);
        }

        return connection;
    }

    private Task<TenantIntegrationConnection?> LoadConnectionAsync(
        Guid tenantId,
        string providerKey,
        CancellationToken cancellationToken) =>
        db.TenantIntegrationConnections
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ProviderKey == TenantIntegrationProviderCatalog.NormalizeProviderKey(providerKey),
                cancellationToken);

    private async Task<Tenant> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken)
        ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

    private async Task<TenantIntegrationConnectionResponse> MapConnectionAsync(
        TenantIntegrationConnection connection,
        Tenant tenant,
        CancellationToken cancellationToken)
    {
        var definition = ResolveProvider(connection.ProviderKey);
        var credential = await db.TenantIntegrationCredentials.AsNoTracking()
            .Where(x => x.ConnectionId == connection.Id)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var health = await db.TenantIntegrationProviderHealth.AsNoTracking()
            .Where(x => x.ConnectionId == connection.Id)
            .OrderByDescending(x => x.CheckedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var syncRun = await db.TenantIntegrationSyncRuns.AsNoTracking()
            .Where(x => x.ConnectionId == connection.Id)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantIntegrationConnectionResponse(
            connection.Id,
            connection.TenantId,
            tenant.Slug,
            tenant.DisplayName,
            definition.ProviderKey,
            definition.DisplayName,
            definition.Category,
            TenantIntegrationProviderCatalog.BuildBrand(definition.ProviderKey, definition.DisplayName),
            connection.Status,
            connection.SyncDirection,
            connection.WritebacksEnabled,
            connection.ManualMappingRequired,
            connection.ConfigurationJson,
            connection.LastSuccessfulSyncAt,
            connection.LastFailedSyncAt,
            connection.LastErrorCategory,
            connection.LastErrorMessage,
            credential is null ? null : MapCredential(credential),
            health is null ? null : new TenantIntegrationHealthResponse(
                health.Status,
                health.CheckedAt,
                health.LatencyMs,
                health.ErrorCategory,
                health.ErrorMessage),
            syncRun is null ? null : MapSyncRun(syncRun),
            TenantIntegrationProviderCatalog.BuildRoutes(definition.ProviderKey),
            connection.CreatedAt,
            connection.UpdatedAt);
    }

    private async Task<bool> CredentialRequiredAndMissingAsync(
        TenantIntegrationConnection connection,
        TenantIntegrationProviderDefinition definition,
        CancellationToken cancellationToken)
    {
        if (definition.ConnectorFamily is "public_api" or "public_or_api" or "file_import")
        {
            return false;
        }

        var hasCredential = await db.TenantIntegrationCredentials
            .AnyAsync(x => x.ConnectionId == connection.Id, cancellationToken);
        return !hasCredential;
    }

    private async Task ValidateSharedSecretAsync(
        Guid? tenantId,
        Guid? connectionId,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (tenantId is null || connectionId is null)
        {
            throw new StlApiException("integrations.connection_required", "A tenant integration connection is required.", 401);
        }

        var provided = context.Request.Headers["X-STL-Integration-Secret"].ToString();
        if (string.IsNullOrWhiteSpace(provided))
        {
            provided = ServiceTokenBearerParser.ParseAuthorizationHeader(
                context.Request.Headers.Authorization.ToString());
        }

        if (string.IsNullOrWhiteSpace(provided))
        {
            throw new StlApiException("integrations.secret_required", "Integration secret is required.", 401);
        }

        var credential = await db.TenantIntegrationCredentials.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ConnectionId == connectionId)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (credential is null)
        {
            throw new StlApiException("integrations.credentials_missing", "Integration credentials were not found.", 401);
        }

        var payload = DecryptCredentialPayload(credential);
        var expected = ResolveString(payload, "webhookSecret", "sharedSecret", "scimBearerToken", "apiKey", "token");
        if (string.IsNullOrWhiteSpace(expected) || !FixedTimeEquals(expected, provided))
        {
            throw new StlApiException("integrations.secret_invalid", "Integration secret is invalid.", 401);
        }
    }

    private async Task ApplyCredentialHeadersAsync(
        HttpRequestMessage request,
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var credential = await db.TenantIntegrationCredentials.AsNoTracking()
            .Where(x => x.ConnectionId == connectionId)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (credential is null)
        {
            return;
        }

        var payload = DecryptCredentialPayload(credential);
        var bearer = ResolveString(payload, "accessToken", "bearerToken", "token");
        if (!string.IsNullOrWhiteSpace(bearer))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer);
            return;
        }

        var apiKey = ResolveString(payload, "apiKey", "clientSecret");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.TryAddWithoutValidation("X-API-Key", apiKey);
        }
    }

    private Dictionary<string, string> DecryptCredentialPayload(TenantIntegrationCredential credential)
    {
        var plaintext = credentialProtector.Unprotect(credential.EncryptedPayload);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(plaintext, JsonOptions)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static TenantIntegrationProviderDefinition ResolveProvider(string providerKey)
    {
        try
        {
            return TenantIntegrationProviderCatalog.GetRequired(providerKey);
        }
        catch (KeyNotFoundException)
        {
            throw new StlApiException(
                "integrations.provider_not_found",
                "Integration provider was not found.",
                404);
        }
    }

    private static string NormalizeConnectionStatus(string raw)
    {
        var status = raw.Trim().ToLowerInvariant();
        if (!TenantIntegrationStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "integrations.invalid_status",
                $"Integration status must be one of: {string.Join(", ", TenantIntegrationStatuses.All)}.",
                400);
        }

        return status;
    }

    private static string NormalizeDirection(string raw)
    {
        var direction = raw.Trim().ToLowerInvariant();
        if (!TenantIntegrationDirections.All.Contains(direction))
        {
            throw new StlApiException(
                "integrations.invalid_direction",
                $"Integration direction must be one of: {string.Join(", ", TenantIntegrationDirections.All)}.",
                400);
        }

        return direction;
    }

    private static string NormalizeCredentialKind(string raw, string fallback)
    {
        var value = string.IsNullOrWhiteSpace(raw) ? fallback : raw.Trim();
        return value.Length > 64 ? value[..64] : value;
    }

    private static string NormalizeJsonObject(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new StlApiException("integrations.json_object_required", "Configuration JSON must be an object.", 400);
            }

            return JsonSerializer.Serialize(document.RootElement, JsonOptions);
        }
        catch (JsonException)
        {
            throw new StlApiException("integrations.invalid_json", "Configuration JSON is invalid.", 400);
        }
    }

    private static IReadOnlyDictionary<string, string> ParseConfiguration(string json)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                result[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return result;
    }

    private static string? ResolveString(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static string BuildRedactedLabel(string secretLabel, IReadOnlyDictionary<string, string> payload)
    {
        var label = string.IsNullOrWhiteSpace(secretLabel) ? "Tenant integration credential" : secretLabel.Trim();
        var value = payload.Values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        var suffix = string.IsNullOrWhiteSpace(value) || value.Length < 4 ? "****" : $"****{value[^4..]}";
        return $"{label} ({suffix})";
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;
        return text;
    }

    private static string ComputeSha256Hex(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private static CallbackState ParseCallbackState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new StlApiException("integrations.oauth_state_required", "OAuth state is required.", 400);
        }

        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlDecode(state));
            var payload = JsonSerializer.Deserialize<CallbackState>(json, JsonOptions);
            if (payload is null || payload.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(payload.ProviderKey))
            {
                throw new StlApiException("integrations.oauth_state_invalid", "OAuth state is invalid.", 400);
            }

            if (payload.IssuedAt.AddHours(1) < DateTimeOffset.UtcNow)
            {
                throw new StlApiException("integrations.oauth_state_expired", "OAuth state is expired.", 400);
            }

            return payload;
        }
        catch (FormatException)
        {
            throw new StlApiException("integrations.oauth_state_invalid", "OAuth state is invalid.", 400);
        }
        catch (JsonException)
        {
            throw new StlApiException("integrations.oauth_state_invalid", "OAuth state is invalid.", 400);
        }
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Length <= maxLength
                ? value
                : value[..maxLength];

    private static TenantIntegrationCredentialSummaryResponse MapCredential(TenantIntegrationCredential credential) =>
        new(
            credential.Id,
            credential.CredentialKind,
            credential.RedactedLabel,
            credential.EncryptionKeyId,
            credential.ExpiresAt,
            credential.LastValidatedAt,
            credential.UpdatedAt);

    private static TenantIntegrationSyncRunResponse MapSyncRun(TenantIntegrationSyncRun x) =>
        new(
            x.Id,
            x.TenantId,
            x.ConnectionId,
            x.ProviderKey,
            x.Status,
            x.Direction,
            x.TriggeredBy,
            x.AttemptCount,
            x.StartedAt,
            x.CompletedAt,
            x.NextRetryAt,
            x.SnapshotCount,
            x.MappingCount,
            x.ErrorCategory,
            x.ErrorMessage,
            x.DestinationProductsJson,
            x.ResultSummaryJson);

    private static TenantIntegrationMappingTemplateResponse MapTemplate(TenantIntegrationManualMappingTemplate x) =>
        new(
            x.Id,
            x.TenantId,
            x.ConnectionId,
            x.ProviderKey,
            x.TemplateName,
            x.SourceEntityType,
            x.TargetProductKey,
            x.TargetEntityType,
            x.MappingJson,
            x.IsActive,
            x.UpdatedAt);

    private static TenantIntegrationExternalMappingResponse MapExternalMapping(TenantIntegrationExternalMapping x) =>
        new(
            x.Id,
            x.TenantId,
            x.ConnectionId,
            x.ProviderKey,
            x.OwningProductKey,
            x.StlEntityType,
            x.StlEntityId,
            x.ExternalEntityType,
            x.ExternalId,
            x.MappingStatus,
            x.SyncDirection,
            x.LastVerifiedAt,
            x.LastSyncAt,
            x.LastError);

    private static TenantIntegrationIntakeAttemptResponse MapIntake(TenantIntegrationIntakeAttempt x) =>
        new(
            x.Id,
            x.TenantId,
            x.ConnectionId,
            x.ProviderKey,
            x.IntakeKind,
            x.IdempotencyKey,
            x.Status,
            x.SourceRoute,
            x.PayloadHash,
            x.FileName,
            x.ContentType,
            x.ReceivedAt,
            x.ProcessedAt,
            x.ErrorCategory,
            x.ErrorMessage);

    private sealed record CallbackState(
        Guid TenantId,
        string ProviderKey,
        string Nonce,
        DateTimeOffset IssuedAt);

    private sealed record ConnectorProbeResult(
        string Status,
        double? LatencyMs,
        string? ErrorCategory,
        string? ErrorMessage,
        string MetadataJson);

    private sealed record ConnectorSyncResult(
        string Status,
        int SnapshotCount,
        string? ErrorCategory,
        string? ErrorMessage,
        object ResultSummary);
}
