using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetInstalledComponentService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue)
{
    private static readonly HashSet<string> AllowedComponentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "engine",
        "transmission",
        "axle",
        "brake_system",
        "tire",
        "wheel",
        "battery",
        "hydraulic",
        "electrical",
        "motor",
        "pump",
        "sensor",
        "safety_device",
        "attachment",
        "belt",
        "filter",
        "cylinder",
        "control_module",
        "other",
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "planned",
        "installed",
        "removed",
        "failed",
        "replaced",
        "retired",
    };

    private static readonly HashSet<string> AllowedConditions = new(StringComparer.OrdinalIgnoreCase)
    {
        "good",
        "fair",
        "poor",
        "failed",
        "unknown",
    };

    public async Task<IReadOnlyList<AssetInstalledComponentResponse>> ListAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var assetExists = await db.Assets.AnyAsync(
            x => x.TenantId == tenantId && x.Id == assetId,
            cancellationToken);
        if (!assetExists)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var components = await db.AssetInstalledComponents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentAssetId == assetId)
            .OrderBy(x => x.ParentComponentId)
            .ThenBy(x => x.ComponentType)
            .ThenBy(x => x.ComponentNumber)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return components.Select(MapResponse).ToList();
    }

    public async Task<AssetInstalledComponentResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid assetId,
        CreateAssetInstalledComponentRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var componentNumber = NormalizeRequiredKey(request.ComponentNumber, "component number", 64);
        var name = NormalizeRequiredKey(request.Name, "component name", 128);
        var componentType = NormalizeRequiredChoice(request.ComponentType, AllowedComponentTypes, "component type");
        var status = NormalizeRequiredChoice(request.Status, AllowedStatuses, "component status");
        var condition = NormalizeRequiredChoice(request.Condition, AllowedConditions, "component condition");
        var parentComponentId = request.ParentComponentId;

        if (parentComponentId is Guid parentId)
        {
            var parentExists = await db.AssetInstalledComponents.AnyAsync(
                x => x.TenantId == tenantId && x.ParentAssetId == assetId && x.Id == parentId,
                cancellationToken);
            if (!parentExists)
            {
                throw new StlApiException("asset_components.parent_not_found", "Parent component was not found for this asset.", 404);
            }
        }

        var duplicate = await db.AssetInstalledComponents.AnyAsync(
            x => x.TenantId == tenantId && x.ParentAssetId == assetId && x.ComponentNumber == componentNumber,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException("asset_components.duplicate_number", "A component with this number already exists on this asset.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssetInstalledComponent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ComponentNumber = componentNumber,
            ParentAssetId = assetId,
            ParentComponentId = parentComponentId,
            Name = name,
            Description = NormalizeOptional(request.Description),
            ComponentType = componentType,
            Status = status,
            Make = NormalizeOptional(request.Make),
            Model = NormalizeOptional(request.Model),
            SerialNumber = NormalizeOptional(request.SerialNumber),
            PartNumberSnapshot = NormalizeOptional(request.PartNumberSnapshot),
            InstalledPartUsageRef = NormalizeOptional(request.InstalledPartUsageRef),
            InstallDate = request.InstallDate,
            InstalledByPersonId = NormalizeOptional(request.InstalledByPersonId),
            InstalledMeterReading = request.InstalledMeterReading,
            RemovedDate = request.RemovedDate,
            RemovedByPersonId = NormalizeOptional(request.RemovedByPersonId),
            RemovedMeterReading = request.RemovedMeterReading,
            RemovalReason = NormalizeOptional(request.RemovalReason),
            WarrantyStartDate = request.WarrantyStartDate,
            WarrantyEndDate = request.WarrantyEndDate,
            ExpectedLifeHours = request.ExpectedLifeHours,
            ExpectedLifeMiles = request.ExpectedLifeMiles,
            ExpectedLifeCycles = request.ExpectedLifeCycles,
            Condition = condition,
            ReplacementPartRefsJson = JsonSerializer.Serialize(request.ReplacementPartRefs ?? []),
            DocumentRefsJson = JsonSerializer.Serialize(request.DocumentRefs ?? []),
            DefectRefsJson = JsonSerializer.Serialize(request.DefectRefs ?? []),
            WorkOrderRefsJson = JsonSerializer.Serialize(request.WorkOrderRefs ?? []),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetInstalledComponents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "asset_component.create",
            tenantId,
            actorUserId,
            "asset_component",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueueCreateEventsAsync(
            tenantId,
            actorUserId,
            asset,
            entity,
            now,
            cancellationToken);

        return MapResponse(entity);
    }

    public async Task<AssetInstalledComponentResponse> UpdateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid assetId,
        Guid componentId,
        UpdateAssetInstalledComponentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetInstalledComponents
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ParentAssetId == assetId && x.Id == componentId,
                cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("asset_components.not_found", "Asset component was not found.", 404);
        }

        if (!string.IsNullOrWhiteSpace(request.ComponentNumber))
        {
            var componentNumber = NormalizeRequiredKey(request.ComponentNumber, "component number", 64);
            var duplicate = await db.AssetInstalledComponents.AnyAsync(
                x => x.TenantId == tenantId
                    && x.ParentAssetId == assetId
                    && x.Id != componentId
                    && x.ComponentNumber == componentNumber,
                cancellationToken);
            if (duplicate)
            {
                throw new StlApiException("asset_components.duplicate_number", "A component with this number already exists on this asset.", 409);
            }

            entity.ComponentNumber = componentNumber;
        }

        var previousStatus = entity.Status;
        if (request.ParentComponentId is Guid parentComponentId)
        {
            if (parentComponentId == componentId)
            {
                throw new StlApiException("asset_components.parent_invalid", "A component cannot be its own parent.", 400);
            }

            var parentExists = await db.AssetInstalledComponents.AnyAsync(
                x => x.TenantId == tenantId && x.ParentAssetId == assetId && x.Id == parentComponentId,
                cancellationToken);
            if (!parentExists)
            {
                throw new StlApiException("asset_components.parent_not_found", "Parent component was not found for this asset.", 404);
            }

            entity.ParentComponentId = parentComponentId;
        }

        entity.Name = ApplyOptionalRequired(request.Name, entity.Name, "component name", 128);
        entity.Description = ApplyOptionalNullable(request.Description, entity.Description);
        entity.ComponentType = ApplyOptionalChoice(request.ComponentType, entity.ComponentType, AllowedComponentTypes, "component type");
        entity.Status = ApplyOptionalChoice(request.Status, entity.Status, AllowedStatuses, "component status");
        entity.Make = ApplyOptionalNullable(request.Make, entity.Make);
        entity.Model = ApplyOptionalNullable(request.Model, entity.Model);
        entity.SerialNumber = ApplyOptionalNullable(request.SerialNumber, entity.SerialNumber);
        entity.PartNumberSnapshot = ApplyOptionalNullable(request.PartNumberSnapshot, entity.PartNumberSnapshot);
        entity.InstalledPartUsageRef = ApplyOptionalNullable(request.InstalledPartUsageRef, entity.InstalledPartUsageRef);
        entity.InstallDate = request.InstallDate ?? entity.InstallDate;
        entity.InstalledByPersonId = ApplyOptionalNullable(request.InstalledByPersonId, entity.InstalledByPersonId);
        entity.InstalledMeterReading = request.InstalledMeterReading ?? entity.InstalledMeterReading;
        entity.RemovedDate = request.RemovedDate ?? entity.RemovedDate;
        entity.RemovedByPersonId = ApplyOptionalNullable(request.RemovedByPersonId, entity.RemovedByPersonId);
        entity.RemovedMeterReading = request.RemovedMeterReading ?? entity.RemovedMeterReading;
        entity.RemovalReason = ApplyOptionalNullable(request.RemovalReason, entity.RemovalReason);
        entity.WarrantyStartDate = request.WarrantyStartDate ?? entity.WarrantyStartDate;
        entity.WarrantyEndDate = request.WarrantyEndDate ?? entity.WarrantyEndDate;
        entity.ExpectedLifeHours = request.ExpectedLifeHours ?? entity.ExpectedLifeHours;
        entity.ExpectedLifeMiles = request.ExpectedLifeMiles ?? entity.ExpectedLifeMiles;
        entity.ExpectedLifeCycles = request.ExpectedLifeCycles ?? entity.ExpectedLifeCycles;
        entity.Condition = ApplyOptionalChoice(request.Condition, entity.Condition, AllowedConditions, "component condition");
        entity.ReplacementPartRefsJson = request.ReplacementPartRefs is not null
            ? JsonSerializer.Serialize(request.ReplacementPartRefs)
            : entity.ReplacementPartRefsJson;
        entity.DocumentRefsJson = request.DocumentRefs is not null
            ? JsonSerializer.Serialize(request.DocumentRefs)
            : entity.DocumentRefsJson;
        entity.DefectRefsJson = request.DefectRefs is not null
            ? JsonSerializer.Serialize(request.DefectRefs)
            : entity.DefectRefsJson;
        entity.WorkOrderRefsJson = request.WorkOrderRefs is not null
            ? JsonSerializer.Serialize(request.WorkOrderRefs)
            : entity.WorkOrderRefsJson;
        var now = DateTimeOffset.UtcNow;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "asset_component.update",
            tenantId,
            actorUserId,
            "asset_component",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        if (!string.Equals(previousStatus, entity.Status, StringComparison.OrdinalIgnoreCase))
        {
            var asset = await db.Assets
                .AsNoTracking()
                .FirstAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
            await EnqueueLifecycleEventAsync(
                tenantId,
                actorUserId,
                asset,
                entity,
                now,
                cancellationToken);
        }

        return MapResponse(entity);
    }

    private static AssetInstalledComponentResponse MapResponse(AssetInstalledComponent entity) =>
        new(
            entity.Id,
            entity.ComponentNumber,
            entity.ParentAssetId,
            entity.ParentComponentId,
            entity.Name,
            entity.Description,
            entity.ComponentType,
            entity.Status,
            entity.Make,
            entity.Model,
            entity.SerialNumber,
            entity.PartNumberSnapshot,
            entity.InstalledPartUsageRef,
            entity.InstallDate,
            entity.InstalledByPersonId,
            entity.InstalledMeterReading,
            entity.RemovedDate,
            entity.RemovedByPersonId,
            entity.RemovedMeterReading,
            entity.RemovalReason,
            entity.WarrantyStartDate,
            entity.WarrantyEndDate,
            entity.ExpectedLifeHours,
            entity.ExpectedLifeMiles,
            entity.ExpectedLifeCycles,
            entity.Condition,
            DeserializeList(entity.ReplacementPartRefsJson),
            DeserializeList(entity.DocumentRefsJson),
            DeserializeList(entity.DefectRefsJson),
            DeserializeList(entity.WorkOrderRefsJson),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizeRequiredKey(string value, string fieldName, int maxLength)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("asset_components.validation", $"{fieldName} is required.", 400);
        }

        if (trimmed.Length > maxLength)
        {
            throw new StlApiException("asset_components.validation", $"{fieldName} must be {maxLength} characters or fewer.", 400);
        }

        return trimmed;
    }

    private static string NormalizeRequiredChoice(string value, ISet<string> allowed, string fieldName)
    {
        var normalized = NormalizeRequiredKey(value, fieldName, 64).ToLowerInvariant();
        if (!allowed.Contains(normalized))
        {
            throw new StlApiException("asset_components.validation", $"{fieldName} is invalid.", 400);
        }

        return normalized;
    }

    private static string ApplyOptionalRequired(string? value, string currentValue, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return currentValue;
        }

        return NormalizeRequiredKey(value, fieldName, maxLength);
    }

    private static string ApplyOptionalChoice(string? value, string currentValue, ISet<string> allowed, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return currentValue;
        }

        return NormalizeRequiredChoice(value, allowed, fieldName);
    }

    private static string? ApplyOptionalNullable(string? value, string? currentValue)
    {
        if (value is null)
        {
            return currentValue;
        }

        return NormalizeOptional(value);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private async Task EnqueueCreateEventsAsync(
        Guid tenantId,
        Guid? actorUserId,
        Asset asset,
        AssetInstalledComponent component,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        await platformOutboxEnqueue.TryEnqueueComponentEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.ComponentCreated,
            component,
            asset,
            actorUserId ?? Guid.Empty,
            occurredAt,
            $"Component created: {component.ComponentNumber} · {component.Name}",
            component.Status,
            cancellationToken: cancellationToken);

        await EnqueueLifecycleEventAsync(
            tenantId,
            actorUserId,
            asset,
            component,
            occurredAt,
            cancellationToken);
    }

    private async Task EnqueueLifecycleEventAsync(
        Guid tenantId,
        Guid? actorUserId,
        Asset asset,
        AssetInstalledComponent component,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var eventKind = component.Status.ToLowerInvariant() switch
        {
            "installed" => MaintenancePlatformOutboxEventKinds.ComponentInstalled,
            "removed" => MaintenancePlatformOutboxEventKinds.ComponentRemoved,
            "failed" => MaintenancePlatformOutboxEventKinds.ComponentFailed,
            "replaced" => MaintenancePlatformOutboxEventKinds.ComponentReplaced,
            "retired" => MaintenancePlatformOutboxEventKinds.ComponentRetired,
            _ => null,
        };

        if (eventKind is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueueComponentEventAsync(
            tenantId,
            eventKind,
            component,
            asset,
            actorUserId ?? Guid.Empty,
            occurredAt,
            $"Component {component.Status}: {component.ComponentNumber} · {component.Name}",
            component.Status,
            cancellationToken: cancellationToken);
    }
}
