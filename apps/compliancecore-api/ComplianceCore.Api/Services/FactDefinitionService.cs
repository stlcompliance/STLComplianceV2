using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class FactDefinitionService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<FactDefinitionResponse> GetByKeyAsync(
        Guid tenantId,
        string factKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = GoverningBodyService.NormalizeKey(factKey, "fact_definitions.validation", "Fact key");
        var entity = await db.FactDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.FactKey == normalizedKey, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("fact_definitions.not_found", "Fact definition was not found.", 404);
        }

        return MapResponse(entity);
    }

    public async Task<IReadOnlyList<FactDefinitionResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.FactDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.Label)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<FactDefinitionResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateFactDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var factKey = GoverningBodyService.NormalizeKey(request.FactKey, "fact_definitions.validation", "Fact key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "fact_definitions.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(request.Description, "fact_definitions.validation");
        var valueType = NormalizeValueType(request.ValueType);

        var exists = await db.FactDefinitions.AnyAsync(
            x => x.TenantId == tenantId && x.FactKey == factKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "fact_definitions.duplicate",
                "A fact definition with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new FactDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FactKey = factKey,
            Label = label,
            Description = description,
            ValueType = valueType,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.FactDefinitions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_definition.create",
            tenantId,
            actorUserId,
            "fact_definition",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<FactDefinitionResponse> UpdateByKeyAsync(
        Guid tenantId,
        Guid? actorUserId,
        string factKey,
        UpdateFactDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = GoverningBodyService.NormalizeKey(factKey, "fact_definitions.validation", "Fact key");
        var entity = await db.FactDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.FactKey == normalizedKey,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("fact_definitions.not_found", "Fact definition was not found.", 404);
        }

        entity.Label = GoverningBodyService.NormalizeLabel(request.Label, "fact_definitions.validation", "Label");
        entity.Description = GoverningBodyService.NormalizeDescription(request.Description, "fact_definitions.validation");
        entity.ValueType = NormalizeValueType(request.ValueType);
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_definition.update",
            tenantId,
            actorUserId,
            "fact_definition",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<FactDefinitionUsageResponse> GetUsageByKeyAsync(
        Guid tenantId,
        string factKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = GoverningBodyService.NormalizeKey(factKey, "fact_definitions.validation", "Fact key");
        var entity = await db.FactDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.FactKey == normalizedKey, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("fact_definitions.not_found", "Fact definition was not found.", 404);
        }

        var requirements = await db.FactRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.FactDefinitionId == entity.Id && x.IsActive)
            .ToListAsync(cancellationToken);
        return new FactDefinitionUsageResponse(
            requirements.Count,
            requirements.Where(x => x.RulePackId.HasValue).Select(x => x.RulePackId!.Value).Distinct().Count(),
            requirements.Where(x => x.CitationId.HasValue).Select(x => x.CitationId!.Value).Distinct().Count());
    }

    public async Task<ValidateFactPayloadResponse> ValidatePayloadAsync(
        Guid tenantId,
        ValidateFactPayloadRequest request,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ValidateFactPayloadItemResponse>();
        var activeDefinitions = await db.FactDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.FactKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var fact in request.Facts)
        {
            var normalizedKey = fact.FactKey.Trim();
            if (!activeDefinitions.TryGetValue(normalizedKey, out var definition) || !definition.IsActive)
            {
                results.Add(new ValidateFactPayloadItemResponse(
                    fact.FactKey,
                    false,
                    "fact_not_found",
                    "Fact key is not defined as an active fact."));
                continue;
            }

            var (isValid, errorCode, errorMessage) = ValidateByValueType(definition.ValueType, fact.Value);
            results.Add(new ValidateFactPayloadItemResponse(
                fact.FactKey,
                isValid,
                errorCode,
                errorMessage));
        }

        return new ValidateFactPayloadResponse(results);
    }

    private static FactDefinitionResponse MapResponse(FactDefinition entity) =>
        new(
            entity.Id,
            entity.FactKey,
            entity.Label,
            entity.Description,
            entity.ValueType,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeValueType(string valueType)
    {
        var normalized = valueType.Trim().ToLowerInvariant();
        if (!FactValueTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "fact_definitions.validation",
                "Fact value type is not recognized.",
                400);
        }

        return normalized;
    }

    private static (bool IsValid, string? ErrorCode, string? ErrorMessage) ValidateByValueType(string valueType, string? value)
    {
        switch (valueType.ToLowerInvariant())
        {
            case FactValueTypes.Boolean:
                return bool.TryParse(value, out _)
                    ? (true, null, null)
                    : (false, "invalid_boolean", "Expected a boolean value.");
            case FactValueTypes.Number:
                return decimal.TryParse(value, out _)
                    ? (true, null, null)
                    : (false, "invalid_number", "Expected a numeric value.");
            case FactValueTypes.Date:
                return DateTimeOffset.TryParse(value, out _)
                    ? (true, null, null)
                    : (false, "invalid_date", "Expected a date or date-time value.");
            default:
                return string.IsNullOrWhiteSpace(value)
                    ? (false, "invalid_string", "Expected a non-empty string value.")
                    : (true, null, null);
        }
    }
}
