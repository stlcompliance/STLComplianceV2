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
}
