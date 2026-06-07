using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenancePartsKitService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<MaintenancePartsKitListResponse> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var kits = await db.Set<MaintenancePartsKit>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.Lines)
            .OrderBy(x => x.KitNumber)
            .ToListAsync(cancellationToken);

        return new MaintenancePartsKitListResponse(kits.Select(Map).ToArray());
    }

    public async Task<MaintenancePartsKitResponse> GetAsync(
        Guid tenantId,
        Guid partsKitId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        return Map(entity);
    }

    public async Task<MaintenancePartsKitResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateMaintenancePartsKitRequest request,
        CancellationToken cancellationToken = default)
    {
        var kitNumber = NormalizeKey(request.KitNumber, "Kit number");
        var title = NormalizeName(request.Title, "Title");
        var description = NormalizeDescription(request.Description);

        var exists = await db.Set<MaintenancePartsKit>()
            .AnyAsync(x => x.TenantId == tenantId && x.KitNumber == kitNumber, cancellationToken);
        if (exists)
        {
            throw new StlApiException("maintenance_parts_kits.duplicate", "A parts kit with this kit number already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new MaintenancePartsKit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            KitNumber = kitNumber,
            Title = title,
            Description = description,
            AssetTypeApplicabilityJson = SerializeList(request.AssetTypeApplicability),
            WorkOrderTypeApplicabilityJson = SerializeList(request.WorkOrderTypeApplicability),
            PmPlanRef = NormalizeOptional(request.PmPlanRef),
            Status = MaintenancePartsKitStatuses.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.create",
            tenantId,
            actorUserId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partsKitId,
        UpdateMaintenancePartsKitRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (entity.Status == MaintenancePartsKitStatuses.Retired)
        {
            throw new StlApiException("maintenance_parts_kits.retired", "Retired parts kits cannot be updated.", 409);
        }

        entity.Title = NormalizeName(request.Title, "Title");
        entity.Description = NormalizeDescription(request.Description);
        entity.AssetTypeApplicabilityJson = SerializeList(request.AssetTypeApplicability);
        entity.WorkOrderTypeApplicabilityJson = SerializeList(request.WorkOrderTypeApplicability);
        entity.PmPlanRef = NormalizeOptional(request.PmPlanRef);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.update",
            tenantId,
            actorUserId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<MaintenancePartsKitResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partsKitId,
        UpdateMaintenancePartsKitStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.status_update",
            tenantId,
            actorUserId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<MaintenancePartsKitLineResponse> AddLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partsKitId,
        CreateMaintenancePartsKitLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var kit = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (kit.Status == MaintenancePartsKitStatuses.Retired)
        {
            throw new StlApiException("maintenance_parts_kits.retired", "Retired parts kits cannot be changed.", 409);
        }

        var entity = new MaintenancePartsKitLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MaintenancePartsKitId = kit.Id,
            ItemRef = NormalizeKey(request.ItemRef, "Item ref"),
            ItemDescriptionSnapshot = NormalizeName(request.ItemDescriptionSnapshot, "Item description"),
            Quantity = NormalizeQuantity(request.Quantity),
            UnitOfMeasure = NormalizeName(request.UnitOfMeasure, "Unit of measure"),
            Required = request.Required,
            SubstituteAllowed = request.SubstituteAllowed,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit_line.create",
            tenantId,
            actorUserId,
            "maintenance_parts_kit_line",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLine(entity);
    }

    public async Task<MaintenancePartsKitLineResponse> UpdateLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partsKitId,
        Guid lineId,
        UpdateMaintenancePartsKitLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadLineAsync(tenantId, partsKitId, lineId, cancellationToken);
        var kit = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (kit.Status == MaintenancePartsKitStatuses.Retired)
        {
            throw new StlApiException("maintenance_parts_kits.retired", "Retired parts kits cannot be changed.", 409);
        }

        entity.ItemDescriptionSnapshot = NormalizeName(request.ItemDescriptionSnapshot, "Item description");
        entity.Quantity = NormalizeQuantity(request.Quantity);
        entity.UnitOfMeasure = NormalizeName(request.UnitOfMeasure, "Unit of measure");
        entity.Required = request.Required;
        entity.SubstituteAllowed = request.SubstituteAllowed;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit_line.update",
            tenantId,
            actorUserId,
            "maintenance_parts_kit_line",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLine(entity);
    }

    public async Task DeleteLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partsKitId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadLineAsync(tenantId, partsKitId, lineId, cancellationToken);
        var kit = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (kit.Status == MaintenancePartsKitStatuses.Retired)
        {
            throw new StlApiException("maintenance_parts_kits.retired", "Retired parts kits cannot be changed.", 409);
        }

        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit_line.delete",
            tenantId,
            actorUserId,
            "maintenance_parts_kit_line",
            lineId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task<MaintenancePartsKit> LoadKitAsync(
        Guid tenantId,
        Guid partsKitId,
        CancellationToken cancellationToken)
    {
        var entity = await db.Set<MaintenancePartsKit>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partsKitId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("maintenance_parts_kits.not_found", "Parts kit was not found.", 404);
        }

        return entity;
    }

    private async Task<MaintenancePartsKitLine> LoadLineAsync(
        Guid tenantId,
        Guid partsKitId,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        var entity = await db.Set<MaintenancePartsKitLine>()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.MaintenancePartsKitId == partsKitId && x.Id == lineId,
                cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("maintenance_parts_kits.line_not_found", "Parts kit line was not found.", 404);
        }

        return entity;
    }

    private static MaintenancePartsKitResponse Map(MaintenancePartsKit entity)
    {
        var lines = entity.Lines
            .OrderBy(x => x.CreatedAt)
            .Select(MapLine)
            .ToArray();

        return new MaintenancePartsKitResponse(
            entity.Id,
            entity.KitNumber,
            entity.Title,
            entity.Description,
            entity.AssetTypeApplicability,
            entity.WorkOrderTypeApplicability,
            entity.PmPlanRef,
            entity.Status,
            entity.LineRefs,
            lines,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static MaintenancePartsKitLineResponse MapLine(MaintenancePartsKitLine entity) =>
        new(
            entity.Id,
            entity.MaintenancePartsKitId,
            entity.ItemRef,
            entity.ItemDescriptionSnapshot,
            entity.Quantity,
            entity.UnitOfMeasure,
            entity.Required,
            entity.SubstituteAllowed,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeKey(string? value, string label)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException("maintenance_parts_kits.validation", $"{label} must be between 2 and 128 characters.", 400);
        }

        return normalized;
    }

    private static string NormalizeName(string? value, string label)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length < 2 || trimmed.Length > 256)
        {
            throw new StlApiException("maintenance_parts_kits.validation", $"{label} must be between 2 and 256 characters.", 400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException("maintenance_parts_kits.validation", "Quantity must be greater than zero.", 400);
        }

        return decimal.Round(quantity, 3, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IReadOnlyList<string> NormalizeList(IReadOnlyList<string>? values) =>
        values is null ? [] : values.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string SerializeList(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize(NormalizeList(values));

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!MaintenancePartsKitStatuses.All.Contains(normalized))
        {
            throw new StlApiException("maintenance_parts_kits.validation", "Status must be draft, active, or retired.", 400);
        }

        return normalized;
    }
}
