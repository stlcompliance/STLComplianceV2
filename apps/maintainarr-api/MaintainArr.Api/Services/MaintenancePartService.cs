using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenancePartService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<IReadOnlyList<MaintenancePartResponse>> ListAsync(
        Guid tenantId,
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.MaintenanceParts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status)
            && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedStatus = NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x =>
                x.PartNumber.ToUpper().Contains(term)
                || x.DisplayName.ToUpper().Contains(term)
                || x.Description.ToUpper().Contains(term)
                || (x.ManufacturerPartNumber != null && x.ManufacturerPartNumber.ToUpper().Contains(term)));
        }

        var entities = await query
            .OrderBy(x => x.PartNumber)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<MaintenancePartResponse> GetAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.MaintenanceParts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken)
            ?? throw new StlApiException("maintenance_parts.not_found", "Maintenance part profile was not found.", 404);

        return Map(entity);
    }

    public async Task<MaintenancePartResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        CreateMaintenancePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedPartNumber = NormalizePartNumber(request.PartNumber);
        await EnsureUniquePartNumberAsync(tenantId, normalizedPartNumber, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var sourceType = NormalizeSourceType(request.SourceType, request.SupplyArrPartId);
        var entity = new MaintenancePart
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartNumber = NormalizeDisplayPartNumber(request.PartNumber),
            NormalizedPartNumber = normalizedPartNumber,
            DisplayName = NormalizeDisplayName(request.DisplayName),
            Description = NormalizeDescription(request.Description),
            CategoryKey = NormalizeCategoryKey(request.CategoryKey),
            UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure),
            Status = NormalizeStatus(request.Status),
            SourceType = sourceType,
            SourceLabel = BuildSourceLabel(sourceType),
            SupplyArrPartId = request.SupplyArrPartId,
            ManufacturerName = NormalizeOptional(request.ManufacturerName, 256),
            ManufacturerPartNumber = NormalizeOptional(request.ManufacturerPartNumber, 128),
            SdsDocumentId = NormalizeOptional(request.SdsDocumentId, 128),
            ComplianceCoreMaterialKey = NormalizeOptional(request.ComplianceCoreMaterialKey, 128),
            ComplianceCoreHazardKeysJson = MaintenancePart.SerializeList(request.ComplianceCoreHazardKeys ?? []),
            Notes = NormalizeOptional(request.Notes, 1024),
            CreatedByPersonId = NormalizeOptional(actorPersonId, 128),
            UpdatedByPersonId = NormalizeOptional(actorPersonId, 128),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.MaintenanceParts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_part.create",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_part",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<MaintenancePartResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partId,
        UpdateMaintenancePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.MaintenanceParts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken)
            ?? throw new StlApiException("maintenance_parts.not_found", "Maintenance part profile was not found.", 404);

        var normalizedPartNumber = NormalizePartNumber(request.PartNumber);
        await EnsureUniquePartNumberAsync(tenantId, normalizedPartNumber, entity.Id, cancellationToken);

        var sourceType = NormalizeSourceType(request.SourceType, request.SupplyArrPartId);
        entity.PartNumber = NormalizeDisplayPartNumber(request.PartNumber);
        entity.NormalizedPartNumber = normalizedPartNumber;
        entity.DisplayName = NormalizeDisplayName(request.DisplayName);
        entity.Description = NormalizeDescription(request.Description);
        entity.CategoryKey = NormalizeCategoryKey(request.CategoryKey);
        entity.UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure);
        entity.Status = NormalizeStatus(request.Status);
        entity.SourceType = sourceType;
        entity.SourceLabel = BuildSourceLabel(sourceType);
        entity.SupplyArrPartId = request.SupplyArrPartId;
        entity.ManufacturerName = NormalizeOptional(request.ManufacturerName, 256);
        entity.ManufacturerPartNumber = NormalizeOptional(request.ManufacturerPartNumber, 128);
        entity.SdsDocumentId = NormalizeOptional(request.SdsDocumentId, 128);
        entity.ComplianceCoreMaterialKey = NormalizeOptional(request.ComplianceCoreMaterialKey, 128);
        entity.ComplianceCoreHazardKeysJson = MaintenancePart.SerializeList(request.ComplianceCoreHazardKeys ?? []);
        entity.Notes = NormalizeOptional(request.Notes, 1024);
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId, 128);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_part.update",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_part",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<MaintenancePartResponse> ArchiveAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.MaintenanceParts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken)
            ?? throw new StlApiException("maintenance_parts.not_found", "Maintenance part profile was not found.", 404);

        entity.Status = MaintenancePartStatuses.Inactive;
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId, 128);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_part.archive",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_part",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    private async Task EnsureUniquePartNumberAsync(
        Guid tenantId,
        string normalizedPartNumber,
        Guid? currentPartId,
        CancellationToken cancellationToken)
    {
        var exists = await db.MaintenanceParts.AnyAsync(
            x =>
                x.TenantId == tenantId
                && x.NormalizedPartNumber == normalizedPartNumber
                && (!currentPartId.HasValue || x.Id != currentPartId.Value),
            cancellationToken);

        if (exists)
        {
            throw new StlApiException(
                "maintenance_parts.duplicate",
                "A maintenance part profile with this normalized part number already exists.",
                409);
        }
    }

    private static MaintenancePartResponse Map(MaintenancePart entity) =>
        new(
            entity.Id,
            entity.PartNumber,
            entity.DisplayName,
            entity.Description,
            entity.CategoryKey,
            entity.UnitOfMeasure,
            entity.Status,
            entity.SourceType,
            entity.SourceLabel,
            entity.SupplyArrPartId,
            entity.ManufacturerName,
            entity.ManufacturerPartNumber,
            entity.SdsDocumentId,
            entity.ComplianceCoreMaterialKey,
            entity.ComplianceCoreHazardKeys,
            entity.Notes,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeDisplayPartNumber(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "maintenance_parts.part_number_too_long",
                "Part number must be 128 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizePartNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException(
                "maintenance_parts.part_number_required",
                "Part number is required.",
                400);
        }

        var normalized = new string(
            value
                .Trim()
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray());

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "maintenance_parts.part_number_invalid",
                "Part number must include at least one letter or number after normalization.",
                400);
        }

        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "maintenance_parts.part_number_too_long",
                "Normalized part number must be 128 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException(
                "maintenance_parts.display_name_required",
                "Display name is required.",
                400);
        }

        var normalized = value.Trim();
        if (normalized.Length > 256)
        {
            throw new StlApiException(
                "maintenance_parts.display_name_too_long",
                "Display name must be 256 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDescription(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length > 1024)
        {
            throw new StlApiException(
                "maintenance_parts.description_too_long",
                "Description must be 1024 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeCategoryKey(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "maintenance" : value.Trim();
        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "maintenance_parts.category_too_long",
                "Category key must be 128 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeUnitOfMeasure(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "each" : value.Trim();
        if (normalized.Length > 64)
        {
            throw new StlApiException(
                "maintenance_parts.unit_of_measure_too_long",
                "Unit of measure must be 64 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? MaintenancePartStatuses.Active
            : value.Trim().ToLowerInvariant();

        if (!MaintenancePartStatuses.All.Contains(normalized))
        {
            throw new StlApiException(
                "maintenance_parts.status_invalid",
                "Status must be draft, active, inactive, or discontinued.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSourceType(string? value, Guid? supplyArrPartId)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? (supplyArrPartId.HasValue ? MaintenancePartSourceTypes.SupplyArrSnapshot : MaintenancePartSourceTypes.Manual)
            : value.Trim().ToLowerInvariant();

        if (!MaintenancePartSourceTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "maintenance_parts.source_type_invalid",
                "Source type must be manual or supplyarr_snapshot.",
                400);
        }

        return normalized;
    }

    private static string BuildSourceLabel(string sourceType) =>
        sourceType switch
        {
            MaintenancePartSourceTypes.SupplyArrSnapshot => "SupplyArr canonical part snapshot",
            _ => "MaintainArr maintenance profile",
        };

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(
                "maintenance_parts.value_too_long",
                $"Field value must be {maxLength} characters or fewer.",
                400);
        }

        return normalized;
    }
}
