using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class FactSourceService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<FactSourceResponse>> ListAsync(
        Guid tenantId,
        Guid? factDefinitionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.FactSources
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (factDefinitionId.HasValue)
        {
            query = query.Where(x => x.FactDefinitionId == factDefinitionId.Value);
        }

        return await query
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Label)
            .Join(
                db.FactDefinitions.AsNoTracking().Where(d => d.TenantId == tenantId),
                source => source.FactDefinitionId,
                definition => definition.Id,
                (source, definition) => MapResponse(source, definition))
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Ok, string? ErrorCode, string? Message)> TryValidateCreateAsync(
        Guid tenantId,
        CreateFactSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await ValidateCreateAsync(tenantId, request, cancellationToken);
            return (true, null, null);
        }
        catch (StlApiException ex)
        {
            return (false, ex.Code, ex.Message);
        }
    }

    public async Task<FactSourceResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateFactSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateCreateAsync(tenantId, request, cancellationToken);

        var sourceKey = GoverningBodyService.NormalizeKey(request.SourceKey, "fact_sources.validation", "Source key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "fact_sources.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(request.Description, "fact_sources.validation");
        var sourceType = NormalizeSourceType(request.SourceType);
        var configJson = NormalizeConfigJson(request.ConfigJson);
        var productKey = NormalizeOptionalProductKey(request.ProductKey);
        var productReference = NormalizeOptionalReference(request.ProductReference);
        ValidateProductFieldsForSourceType(sourceType, productKey, productReference);

        var definition = await db.FactDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.FactDefinitionId && x.IsActive,
            cancellationToken)
            ?? throw new StlApiException("fact_definitions.not_found", "Fact definition was not found.", 404);

        var now = DateTimeOffset.UtcNow;
        var entity = new FactSource
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FactDefinitionId = definition.Id,
            SourceKey = sourceKey,
            SourceType = sourceType,
            Label = label,
            Description = description,
            ProductKey = productKey,
            ProductReference = productReference,
            ConfigJson = configJson,
            Priority = request.Priority,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.FactSources.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_source.create",
            tenantId,
            actorUserId,
            "fact_source",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity, definition);
    }

    public async Task<FactSourceResponse> UpdateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid factSourceId,
        UpdateFactSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.FactSources.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == factSourceId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("fact_sources.not_found", "Fact source was not found.", 404);
        }

        var definition = await db.FactDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == entity.FactDefinitionId,
            cancellationToken)
            ?? throw new StlApiException("fact_definitions.not_found", "Fact definition was not found.", 404);

        var label = GoverningBodyService.NormalizeLabel(request.Label, "fact_sources.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(request.Description, "fact_sources.validation");
        var configJson = NormalizeConfigJson(request.ConfigJson);
        var productKey = NormalizeOptionalProductKey(request.ProductKey);
        var productReference = NormalizeOptionalReference(request.ProductReference);
        ValidateProductFieldsForSourceType(entity.SourceType, productKey, productReference);

        ValidateConfigForSourceType(entity.SourceType, definition.ValueType, configJson);

        entity.Label = label;
        entity.Description = description;
        entity.ConfigJson = configJson;
        entity.ProductKey = productKey;
        entity.ProductReference = productReference;
        entity.Priority = request.Priority;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_source.update",
            tenantId,
            actorUserId,
            "fact_source",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity, definition);
    }

    private async Task ValidateCreateAsync(
        Guid tenantId,
        CreateFactSourceRequest request,
        CancellationToken cancellationToken)
    {
        var sourceKey = GoverningBodyService.NormalizeKey(request.SourceKey, "fact_sources.validation", "Source key");
        var sourceType = NormalizeSourceType(request.SourceType);
        var configJson = NormalizeConfigJson(request.ConfigJson);

        var definition = await db.FactDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.FactDefinitionId && x.IsActive,
            cancellationToken);

        if (definition is null)
        {
            throw new StlApiException("fact_definitions.not_found", "Fact definition was not found.", 404);
        }

        ValidateConfigForSourceType(sourceType, definition.ValueType, configJson);

        var duplicate = await db.FactSources.AnyAsync(
            x => x.TenantId == tenantId && x.SourceKey == sourceKey,
            cancellationToken);

        if (duplicate)
        {
            throw new StlApiException(
                "fact_sources.duplicate",
                "A fact source with this key already exists.",
                409);
        }
    }

    private static FactSourceResponse MapResponse(FactSource entity, FactDefinition definition) =>
        new(
            entity.Id,
            entity.FactDefinitionId,
            definition.FactKey,
            definition.Label,
            entity.SourceKey,
            entity.SourceType,
            entity.Label,
            entity.Description,
            entity.ProductKey,
            entity.ProductReference,
            entity.ConfigJson,
            entity.Priority,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeSourceType(string sourceType)
    {
        var normalized = sourceType.Trim().ToLowerInvariant();
        if (!FactSourceTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "fact_sources.validation",
                "Fact source type is not recognized.",
                400);
        }

        return normalized;
    }

    private static string NormalizeConfigJson(string configJson)
    {
        var trimmed = configJson?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "{}";
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(trimmed);
            return trimmed;
        }
        catch (System.Text.Json.JsonException)
        {
            throw new StlApiException(
                "fact_sources.validation",
                "Config JSON is not valid.",
                400);
        }
    }

    private static string? NormalizeOptionalProductKey(string? productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return null;
        }

        return productKey.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalReference(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return null;
        }

        var trimmed = reference.Trim();
        if (trimmed.Length > 256)
        {
            throw new StlApiException(
                "fact_sources.validation",
                "Product reference must be 256 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static void ValidateConfigForSourceType(string sourceType, string valueType, string configJson)
    {
        if (string.Equals(sourceType, FactSourceTypes.StaticConfig, StringComparison.Ordinal))
        {
            FactResolver.TryReadStaticValue(valueType, configJson, out _, out var error);
            if (error is not null)
            {
                throw new StlApiException("fact_sources.validation", error, 400);
            }

            return;
        }

        if (string.Equals(sourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal))
        {
            FactSourceApiSyncConfigParser.ValidateForSourceType(sourceType, valueType, configJson, "tenant");
            return;
        }

        if (string.Equals(sourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            FactSourceApiSyncConfigParser.ValidateForSourceType(sourceType, valueType, configJson, "tenant");
            return;
        }

        if (string.Equals(sourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
        {
            return;
        }
    }

    private static void ValidateProductFieldsForSourceType(string sourceType, string? productKey, string? productReference)
    {
        if (!string.Equals(sourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(productKey))
        {
            throw new StlApiException(
                "fact_sources.validation",
                "Generated report sources require a source product.",
                400);
        }

        if (string.IsNullOrWhiteSpace(productReference))
        {
            throw new StlApiException(
                "fact_sources.validation",
                "Generated report sources require a product reference.",
                400);
        }
    }
}
