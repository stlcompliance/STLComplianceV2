using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenancePartsKitService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit,
    SupplyArrSupplyReadinessClient supplyReadinessClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

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

        return new MaintenancePartsKitListResponse(kits.Select(entity => Map(entity, includeDefinition: false)).ToArray());
    }

    public async Task<MaintenancePartsKitResponse> GetAsync(
        Guid tenantId,
        Guid partsKitId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        return Map(entity, includeDefinition: true);
    }

    public async Task<MaintenancePartsKitResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        CreateMaintenancePartsKitRequest request,
        CancellationToken cancellationToken = default)
    {
        var kitNumber = NormalizeKey(request.KitNumber, "Kit number");
        var title = NormalizeName(request.Title, "Title");
        var description = NormalizeDescription(request.Description);
        var tags = NormalizeList(request.Tags);
        var assetTypeApplicability = NormalizeList(request.AssetTypeApplicability);
        var workOrderTypeApplicability = NormalizeList(request.WorkOrderTypeApplicability);
        var definition = NormalizeDefinition(request.Definition);

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
            KitCategoryKey = NormalizeOptional(request.KitCategoryKey),
            KitTypeKey = NormalizeOptional(request.KitTypeKey),
            PriorityKey = NormalizeOptional(request.PriorityKey),
            OwningSiteRef = NormalizeOptional(request.OwningSiteRef),
            OwningTeamRef = NormalizeOptional(request.OwningTeamRef),
            OwnerPersonId = NormalizeOptional(request.OwnerPersonId),
            OwnerRoleKey = NormalizeOptional(request.OwnerRoleKey),
            TagsJson = SerializeList(tags),
            AssetTypeApplicabilityJson = SerializeList(assetTypeApplicability),
            WorkOrderTypeApplicabilityJson = SerializeList(workOrderTypeApplicability),
            PmPlanRef = NormalizeOptional(request.PmPlanRef),
            DefinitionJson = SerializeJson(definition),
            Status = MaintenancePartsKitStatuses.Draft,
            Version = 1,
            CloneSourcePartsKitId = NormalizeOptional(request.CloneSourcePartsKitId),
            CreatedByPersonId = NormalizeOptional(actorPersonId),
            UpdatedByPersonId = NormalizeOptional(actorPersonId),
            EffectiveAt = request.EffectiveAt,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Add(entity);
        await SyncLegacyLinesAsync(entity, definition.Items, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.create",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        UpdateMaintenancePartsKitRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (entity.Status == MaintenancePartsKitStatuses.Retired || entity.Status == MaintenancePartsKitStatuses.Active)
        {
            throw new StlApiException(
                "maintenance_parts_kits.locked",
                "Active or retired parts kits must be cloned before editing.",
                409);
        }

        var existingDefinition = DeserializeDefinition(entity.DefinitionJson);
        var definition = request.Definition is null
            ? existingDefinition
            : NormalizeDefinition(request.Definition);

        entity.Title = NormalizeName(request.Title, "Title");
        entity.Description = NormalizeDescription(request.Description);
        entity.KitCategoryKey = NormalizeOptional(request.KitCategoryKey);
        entity.KitTypeKey = NormalizeOptional(request.KitTypeKey);
        entity.PriorityKey = NormalizeOptional(request.PriorityKey);
        entity.OwningSiteRef = NormalizeOptional(request.OwningSiteRef);
        entity.OwningTeamRef = NormalizeOptional(request.OwningTeamRef);
        entity.OwnerPersonId = NormalizeOptional(request.OwnerPersonId);
        entity.OwnerRoleKey = NormalizeOptional(request.OwnerRoleKey);
        entity.TagsJson = SerializeList(NormalizeList(request.Tags));
        entity.AssetTypeApplicabilityJson = SerializeList(request.AssetTypeApplicability);
        entity.WorkOrderTypeApplicabilityJson = SerializeList(request.WorkOrderTypeApplicability);
        entity.PmPlanRef = NormalizeOptional(request.PmPlanRef);
        entity.DefinitionJson = SerializeJson(definition);
        entity.EffectiveAt = request.EffectiveAt;
        entity.ExpiresAt = request.ExpiresAt;
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (request.Definition is not null)
        {
            await SyncLegacyLinesAsync(entity, definition.Items, cancellationToken);
        }
        entity.Version += 1;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.update",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, partsKitId, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        UpdateMaintenancePartsKitStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        entity.Status = status;
        if (status == MaintenancePartsKitStatuses.Active)
        {
            entity.ActivatedAt = DateTimeOffset.UtcNow;
            entity.ActivatedByPersonId = NormalizeOptional(actorPersonId);
        }
        else if (status == MaintenancePartsKitStatuses.PendingApproval)
        {
            entity.ApprovedAt = null;
            entity.ApprovedByPersonId = null;
        }
        else if (status == MaintenancePartsKitStatuses.Retired || status == MaintenancePartsKitStatuses.Archived)
        {
            entity.RetiredAt = DateTimeOffset.UtcNow;
            entity.RetiredByPersonId = NormalizeOptional(actorPersonId);
        }
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.status_update",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, partsKitId, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> SubmitForApprovalAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (entity.Status == MaintenancePartsKitStatuses.Retired)
        {
            throw new StlApiException("maintenance_parts_kits.retired", "Retired parts kits cannot be submitted.", 409);
        }

        entity.Status = MaintenancePartsKitStatuses.PendingApproval;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.submit_approval",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, partsKitId, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> ActivateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (entity.Status == MaintenancePartsKitStatuses.Retired)
        {
            throw new StlApiException("maintenance_parts_kits.retired", "Retired parts kits cannot be activated.", 409);
        }

        var analysis = await AnalyzeDefinitionAsync(
            tenantId,
            BuildPreviewInputFromEntity(entity),
            forActivation: true,
            cancellationToken);
        if (!analysis.Validation.IsValid)
        {
            throw new StlApiException("maintenance_parts_kits.validation", string.Join(" ", analysis.Validation.Errors), 400);
        }

        var definition = DeserializeDefinition(entity.DefinitionJson);
        if (definition.Approval.RequiresApprovalBeforeActivation && !string.Equals(entity.Status, MaintenancePartsKitStatuses.PendingApproval, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "maintenance_parts_kits.approval_required",
                "This kit must be submitted for approval before activation.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = MaintenancePartsKitStatuses.Active;
        entity.ActivatedAt = now;
        entity.ActivatedByPersonId = NormalizeOptional(actorPersonId);
        if (definition.Approval.RequiresApprovalBeforeActivation)
        {
            entity.ApprovedAt ??= now;
            entity.ApprovedByPersonId ??= NormalizeOptional(actorPersonId);
        }
        entity.UpdatedAt = now;
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.activate",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, partsKitId, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> RetireAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (entity.Status == MaintenancePartsKitStatuses.Retired)
        {
            return await GetAsync(tenantId, partsKitId, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = MaintenancePartsKitStatuses.Retired;
        entity.RetiredAt = now;
        entity.RetiredByPersonId = NormalizeOptional(actorPersonId);
        entity.UpdatedAt = now;
        entity.UpdatedByPersonId = NormalizeOptional(actorPersonId);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.retire",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, partsKitId, cancellationToken);
    }

    public async Task<MaintenancePartsKitResponse> CloneAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        CancellationToken cancellationToken = default)
    {
        var source = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        var sourceDefinition = DeserializeDefinition(source.DefinitionJson);
        var now = DateTimeOffset.UtcNow;
        var clone = new MaintenancePartsKit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            KitNumber = await GenerateCloneKitNumberAsync(tenantId, source.KitNumber, cancellationToken),
            Title = source.Title,
            Description = source.Description,
            KitCategoryKey = source.KitCategoryKey,
            KitTypeKey = source.KitTypeKey,
            PriorityKey = source.PriorityKey,
            OwningSiteRef = source.OwningSiteRef,
            OwningTeamRef = source.OwningTeamRef,
            OwnerPersonId = source.OwnerPersonId,
            OwnerRoleKey = source.OwnerRoleKey,
            TagsJson = source.TagsJson,
            AssetTypeApplicabilityJson = source.AssetTypeApplicabilityJson,
            WorkOrderTypeApplicabilityJson = source.WorkOrderTypeApplicabilityJson,
            PmPlanRef = source.PmPlanRef,
            DefinitionJson = source.DefinitionJson,
            Status = MaintenancePartsKitStatuses.Draft,
            Version = source.Version + 1,
            CloneSourcePartsKitId = source.Id.ToString("D"),
            EffectiveAt = source.EffectiveAt,
            ExpiresAt = source.ExpiresAt,
            CreatedByPersonId = NormalizeOptional(actorPersonId),
            UpdatedByPersonId = NormalizeOptional(actorPersonId),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Add(clone);
        await SyncLegacyLinesAsync(clone, sourceDefinition.Items, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit.clone",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit",
            clone.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, clone.Id, cancellationToken);
    }

    public async Task<MaintenancePartsKitValidationResponse> ValidateAsync(
        Guid tenantId,
        MaintenancePartsKitPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var draft = NormalizePreviewRequest(request);
        var analysis = await AnalyzeDefinitionAsync(
            tenantId,
            draft,
            forActivation: false,
            cancellationToken);
        return analysis.Validation;
    }

    public async Task<MaintenancePartsKitPreviewResponse> PreviewAsync(
        Guid tenantId,
        MaintenancePartsKitPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var draft = NormalizePreviewRequest(request);
        var analysis = await AnalyzeDefinitionAsync(
            tenantId,
            draft,
            forActivation: false,
            cancellationToken);
        return analysis.Preview;
    }

    public async Task<MaintenancePartsKitLineResponse> AddLineAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        CreateMaintenancePartsKitLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var kit = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (kit.Status == MaintenancePartsKitStatuses.Retired || kit.Status == MaintenancePartsKitStatuses.Active)
        {
            throw new StlApiException("maintenance_parts_kits.locked", "Active or retired parts kits cannot be changed.", 409);
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
            SortOrder = kit.Lines.Count == 0 ? 0 : kit.Lines.Max(x => x.SortOrder) + 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit_line.create",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit_line",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLine(entity, null);
    }

    public async Task<MaintenancePartsKitLineResponse> UpdateLineAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        Guid lineId,
        UpdateMaintenancePartsKitLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadLineAsync(tenantId, partsKitId, lineId, cancellationToken);
        var kit = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (kit.Status == MaintenancePartsKitStatuses.Retired || kit.Status == MaintenancePartsKitStatuses.Active)
        {
            throw new StlApiException("maintenance_parts_kits.locked", "Active or retired parts kits cannot be changed.", 409);
        }

        entity.ItemDescriptionSnapshot = NormalizeName(request.ItemDescriptionSnapshot, "Item description");
        entity.Quantity = NormalizeQuantity(request.Quantity);
        entity.UnitOfMeasure = NormalizeName(request.UnitOfMeasure, "Unit of measure");
        entity.Required = request.Required;
        entity.SubstituteAllowed = request.SubstituteAllowed;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.SortOrder = entity.SortOrder;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit_line.update",
            tenantId,
            actorUserId,
            actorPersonId,
            "maintenance_parts_kit_line",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLine(entity, null);
    }

    public async Task DeleteLineAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid partsKitId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadLineAsync(tenantId, partsKitId, lineId, cancellationToken);
        var kit = await LoadKitAsync(tenantId, partsKitId, cancellationToken);
        if (kit.Status == MaintenancePartsKitStatuses.Retired || kit.Status == MaintenancePartsKitStatuses.Active)
        {
            throw new StlApiException("maintenance_parts_kits.locked", "Active or retired parts kits cannot be changed.", 409);
        }

        db.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "maintenance_parts_kit_line.delete",
            tenantId,
            actorUserId,
            actorPersonId,
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

    private static MaintenancePartsKitResponse Map(MaintenancePartsKit entity, bool includeDefinition)
    {
        var definition = includeDefinition
            ? DeserializeDefinition(entity.DefinitionJson)
            : null;
        var itemLookup = definition is null
            ? null
            : definition.Items.ToDictionary(item => ResolveOwnershipItemRef(item), StringComparer.OrdinalIgnoreCase);
        var lines = entity.Lines
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(line => MapLine(
                line,
                itemLookup is null || !itemLookup.TryGetValue(line.ItemRef, out var snapshot) ? null : snapshot))
            .ToArray();

        return new MaintenancePartsKitResponse(
            entity.Id,
            entity.KitNumber,
            entity.Title,
            entity.Description,
            entity.KitCategoryKey,
            entity.KitTypeKey,
            entity.PriorityKey,
            entity.OwningSiteRef,
            entity.OwningTeamRef,
            entity.OwnerPersonId,
            entity.OwnerRoleKey,
            entity.Tags,
            entity.AssetTypeApplicability,
            entity.WorkOrderTypeApplicability,
            entity.PmPlanRef,
            definition,
            entity.Status,
            entity.Version,
            entity.LineRefs,
            lines,
            entity.EffectiveAt,
            entity.ExpiresAt,
            entity.ActivatedAt,
            entity.ApprovedAt,
            entity.RetiredAt,
            entity.CloneSourcePartsKitId,
            entity.CreatedByPersonId,
            entity.UpdatedByPersonId,
            entity.ActivatedByPersonId,
            entity.ApprovedByPersonId,
            entity.RetiredByPersonId,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static MaintenancePartsKitLineResponse MapLine(
        MaintenancePartsKitLine entity,
        MaintenancePartsKitItemResponse? snapshot) =>
        new(
            entity.Id,
            entity.MaintenancePartsKitId,
            entity.ItemRef,
            entity.ItemDescriptionSnapshot,
            entity.Quantity,
            entity.UnitOfMeasure,
            entity.Required,
            entity.SubstituteAllowed,
            entity.SortOrder,
            snapshot?.SupplyarrPartId,
            snapshot?.PartNumberSnapshot,
            snapshot?.ManufacturerPartNumberSnapshot,
            snapshot?.VendorPartNumberSnapshot,
            snapshot?.Criticality ?? "medium",
            snapshot?.Consumable ?? false,
            snapshot?.Serialized ?? false,
            snapshot?.CoreReturnExpected ?? false,
            snapshot?.Hazardous ?? false,
            snapshot?.WarrantySensitive ?? false,
            snapshot?.RequiredByTask,
            snapshot?.Notes,
            snapshot?.Tags ?? [],
            snapshot?.PreferredSubstituteRefs ?? [],
            snapshot?.IsPlaceholder ?? false,
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

    private static string NormalizeText(string? value, int maxLength = 1024) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()[..Math.Min(value.Trim().Length, maxLength)];

    private static string NormalizeDescription(string? value) =>
        NormalizeText(value, 1024);

    private static decimal NormalizeQuantity(decimal quantity, bool allowZero = false)
    {
        if (quantity < 0 || (!allowZero && quantity <= 0))
        {
            throw new StlApiException("maintenance_parts_kits.validation", "Quantity must be greater than zero.", 400);
        }

        return decimal.Round(quantity, 3, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ResolveOwnershipItemRef(MaintenancePartsKitItemResponse item) =>
        !string.IsNullOrWhiteSpace(item.SupplyarrPartId) ? item.SupplyarrPartId : item.ItemRef;

    private static IReadOnlyList<string> NormalizeList(IReadOnlyList<string>? values) =>
        values is null ? [] : values.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string SerializeList(IReadOnlyList<string>? values) =>
        JsonSerializer.Serialize(NormalizeList(values), JsonOptions);

    private static string SerializeJson<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static T? DeserializeJson<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static MaintenancePartsKitDefinitionResponse DeserializeDefinition(string? json)
    {
        return DeserializeJson<MaintenancePartsKitDefinitionResponse>(json)
            ?? NormalizeDefinition((MaintenancePartsKitDefinitionRequest?)null);
    }

    private static MaintenancePartsKitDefinitionResponse NormalizeDefinition(MaintenancePartsKitDefinitionRequest? request)
    {
        var assetScope = NormalizeAssetScope(request?.AssetScope);
        var items = NormalizeItems(request?.Items);
        var rules = NormalizeQuantityRules(request?.QuantityRules);

        return new MaintenancePartsKitDefinitionResponse(
            NormalizeList(request?.ApplicabilityWorkOrderTypes),
            NormalizeList(request?.ApplicabilityPmProgramRefs),
            NormalizeList(request?.ApplicabilityInspectionTemplateRefs),
            NormalizeList(request?.ApplicabilityDefectTypes),
            NormalizeList(request?.ApplicabilityTaskTemplateRefs),
            NormalizeList(request?.ApplicabilityRepairCategories),
            NormalizeList(request?.WorkSourceCompatibilities),
            assetScope,
            items,
            rules,
            NormalizeAvailability(request?.Availability),
            NormalizeWorkOrderBehavior(request?.WorkOrderBehavior),
            NormalizeCompliance(request?.Compliance),
            NormalizeApproval(request?.Approval),
            NormalizeOptional(request?.ChangeReason),
            NormalizeOptional(request?.VersionLabel));
    }

    private static MaintenancePartsKitDefinitionResponse NormalizeDefinition(MaintenancePartsKitDefinitionResponse? response)
    {
        if (response is null)
        {
            return NormalizeDefinition((MaintenancePartsKitDefinitionRequest?)null);
        }

        return NormalizeDefinition(new MaintenancePartsKitDefinitionRequest(
            response.ApplicabilityWorkOrderTypes,
            response.ApplicabilityPmProgramRefs,
            response.ApplicabilityInspectionTemplateRefs,
            response.ApplicabilityDefectTypes,
            response.ApplicabilityTaskTemplateRefs,
            response.ApplicabilityRepairCategories,
            response.WorkSourceCompatibilities,
            new MaintenancePartsKitAssetScopeRequest(
                response.AssetScope.AssetClassKeys,
                response.AssetScope.AssetTypeKeys,
                response.AssetScope.AssetCategoryKeys,
                response.AssetScope.AssetStatusKeys,
                response.AssetScope.SiteRefs,
                response.AssetScope.DepartmentRefs,
                response.AssetScope.MakeKeys,
                response.AssetScope.ModelKeys,
                response.AssetScope.YearFrom,
                response.AssetScope.YearTo,
                response.AssetScope.FuelTypeKeys,
                response.AssetScope.BodyTypeKeys,
                response.AssetScope.ConfigurationKeys,
                response.AssetScope.VariantFlags,
                response.AssetScope.RequiredAttributes,
                response.AssetScope.ExcludedAttributes,
                response.AssetScope.IncludedAssetIds,
                response.AssetScope.ExcludedAssetIds),
            response.Items.Select(item => new MaintenancePartsKitItemRequest(
                item.ItemRef,
                item.SupplyarrPartId,
                item.ItemDescriptionSnapshot,
                item.PartNumberSnapshot,
                item.ManufacturerPartNumberSnapshot,
                item.VendorPartNumberSnapshot,
                item.Quantity,
                item.UnitOfMeasure,
                item.Required,
                item.Criticality,
                item.SubstituteAllowed,
                item.PreferredSubstituteRefs,
                item.Consumable,
                item.Serialized,
                item.CoreReturnExpected,
                item.Hazardous,
                item.WarrantySensitive,
                item.RequiredByTask,
                item.Notes,
                item.Tags,
                item.IsPlaceholder)).ToArray(),
            response.QuantityRules.Select(rule => new MaintenancePartsKitQuantityRuleRequest(
                rule.RuleId,
                rule.RuleType,
                rule.AppliesToItemRef,
                rule.AssetConditionSummary,
                rule.WorkConditionSummary,
                rule.ConditionSummary,
                rule.BaseQuantity,
                rule.Multiplier,
                rule.MinimumQuantity,
                rule.MaximumQuantity,
                rule.RoundingBehavior,
                rule.PlainLanguageSummary)).ToArray(),
            new MaintenancePartsKitAvailabilityRequest(
                response.Availability.Enabled,
                response.Availability.PreferredFulfillmentSource,
                response.Availability.ShowSiteAvailability,
                response.Availability.ShowNearbyAvailability,
                response.Availability.ShowOnOrder,
                response.Availability.ShowEstimatedLeadTime,
                response.Availability.RequestReservation,
                response.Availability.Notes),
            new MaintenancePartsKitWorkOrderBehaviorRequest(
                response.WorkOrderBehavior.CanBeManuallyAdded,
                response.WorkOrderBehavior.AutoSuggestOnMatchingWorkOrder,
                response.WorkOrderBehavior.AutoAddToMatchingWorkOrder,
                response.WorkOrderBehavior.AutoAddToPmGeneratedWorkOrder,
                response.WorkOrderBehavior.AutoAddAfterFailedInspectionQuestion,
                response.WorkOrderBehavior.AutoAddAfterMatchingDefectType,
                response.WorkOrderBehavior.RequireSupervisorApprovalBeforeAdding,
                response.WorkOrderBehavior.RequirePartsReviewBeforeWorkCanStart,
                response.WorkOrderBehavior.RequireAvailabilityCheckBeforeScheduling,
                response.WorkOrderBehavior.AllowTechnicianAdjustQuantities,
                response.WorkOrderBehavior.RequireAdjustmentReason,
                response.WorkOrderBehavior.AllowTechnicianRemoveOptionalItems,
                response.WorkOrderBehavior.AllowTechnicianRemoveRequiredItems,
                response.WorkOrderBehavior.RequireReasonToRemoveRequiredItem,
                response.WorkOrderBehavior.SnapshotKitItemsOntoWorkOrder,
                response.WorkOrderBehavior.KeepLiveReferenceAfterWorkOrderCreation),
            new MaintenancePartsKitComplianceRequest(
                response.Compliance.ComplianceRelated,
                response.Compliance.GoverningBodyKeys,
                response.Compliance.CitationRefs,
                response.Compliance.SafetyCritical,
                response.Compliance.ReadinessSensitive,
                response.Compliance.MissingRequiredPartsBlockWorkStart,
                response.Compliance.MissingRequiredPartsBlockWorkCompletion,
                response.Compliance.RequireSupervisorApprovalForSubstitution,
                response.Compliance.RequireDocumentationForSubstitution,
                response.Compliance.RequireFinalInspectionAfterUse,
                response.Compliance.LinkedInspectionTemplateId),
            new MaintenancePartsKitApprovalRequest(
                response.Approval.RequiresApprovalBeforeActivation,
                response.Approval.ApproverRoleKey,
                response.Approval.ApproverPersonId,
                response.Approval.RetireReplacedKitAfterActivation,
                response.Approval.NotesForApprover),
            response.ChangeReason,
            response.VersionLabel));
    }

    private static MaintenancePartsKitAssetScopeResponse NormalizeAssetScope(MaintenancePartsKitAssetScopeRequest? request) =>
        new(
            NormalizeList(request?.AssetClassKeys),
            NormalizeList(request?.AssetTypeKeys),
            NormalizeList(request?.AssetCategoryKeys),
            NormalizeList(request?.AssetStatusKeys),
            NormalizeList(request?.SiteRefs),
            NormalizeList(request?.DepartmentRefs),
            NormalizeList(request?.MakeKeys),
            NormalizeList(request?.ModelKeys),
            NormalizeOptional(request?.YearFrom),
            NormalizeOptional(request?.YearTo),
            NormalizeList(request?.FuelTypeKeys),
            NormalizeList(request?.BodyTypeKeys),
            NormalizeList(request?.ConfigurationKeys),
            NormalizeList(request?.VariantFlags),
            NormalizeList(request?.RequiredAttributes),
            NormalizeList(request?.ExcludedAttributes),
            NormalizeList(request?.IncludedAssetIds),
            NormalizeList(request?.ExcludedAssetIds));

    private static IReadOnlyList<MaintenancePartsKitItemResponse> NormalizeItems(IReadOnlyList<MaintenancePartsKitItemRequest>? items)
    {
        if (items is null)
        {
            return [];
        }

        return items.Select((item, index) =>
        {
            var itemRef = string.IsNullOrWhiteSpace(item.ItemRef)
                ? $"placeholder-{Guid.NewGuid():N}"
                : item.ItemRef.Trim();
            var isPlaceholder = item.IsPlaceholder || itemRef.StartsWith("placeholder-", StringComparison.OrdinalIgnoreCase);
            return new MaintenancePartsKitItemResponse(
                itemRef,
                NormalizeOptional(item.SupplyarrPartId),
                NormalizeText(item.ItemDescriptionSnapshot, 512),
                NormalizeOptional(item.PartNumberSnapshot),
                NormalizeOptional(item.ManufacturerPartNumberSnapshot),
                NormalizeOptional(item.VendorPartNumberSnapshot),
                NormalizeQuantity(item.Quantity, allowZero: true),
                NormalizeText(item.UnitOfMeasure, 64),
                item.Required,
                string.IsNullOrWhiteSpace(item.Criticality) ? "medium" : item.Criticality.Trim().ToLowerInvariant(),
                item.SubstituteAllowed,
                NormalizeList(item.PreferredSubstituteRefs),
                item.Consumable,
                item.Serialized,
                item.CoreReturnExpected,
                item.Hazardous,
                item.WarrantySensitive,
                NormalizeOptional(item.RequiredByTask),
                NormalizeOptional(item.Notes),
                NormalizeList(item.Tags),
                isPlaceholder);
        }).ToArray();
    }

    private static IReadOnlyList<MaintenancePartsKitQuantityRuleResponse> NormalizeQuantityRules(IReadOnlyList<MaintenancePartsKitQuantityRuleRequest>? rules)
    {
        if (rules is null)
        {
            return [];
        }

        return rules.Select(rule => new MaintenancePartsKitQuantityRuleResponse(
            NormalizeOptional(rule.RuleId) ?? $"rule-{Guid.NewGuid():N}",
            NormalizeText(rule.RuleType, 64).ToLowerInvariant(),
            NormalizeText(rule.AppliesToItemRef, 128),
            NormalizeOptional(rule.AssetConditionSummary),
            NormalizeOptional(rule.WorkConditionSummary),
            NormalizeOptional(rule.ConditionSummary),
            NormalizeQuantity(rule.BaseQuantity, allowZero: true),
            NormalizeQuantity(rule.Multiplier, allowZero: true),
            rule.MinimumQuantity is null ? null : NormalizeQuantity(rule.MinimumQuantity.Value, allowZero: true),
            rule.MaximumQuantity is null ? null : NormalizeQuantity(rule.MaximumQuantity.Value, allowZero: true),
            NormalizeText(rule.RoundingBehavior, 32).ToLowerInvariant(),
            NormalizeText(rule.PlainLanguageSummary, 512))).ToArray();
    }

    private static MaintenancePartsKitAvailabilityResponse NormalizeAvailability(MaintenancePartsKitAvailabilityRequest? request) =>
        new(
            request?.Enabled ?? false,
            NormalizeOptional(request?.PreferredFulfillmentSource),
            request?.ShowSiteAvailability ?? false,
            request?.ShowNearbyAvailability ?? false,
            request?.ShowOnOrder ?? false,
            request?.ShowEstimatedLeadTime ?? false,
            request?.RequestReservation ?? false,
            NormalizeOptional(request?.Notes));

    private static MaintenancePartsKitWorkOrderBehaviorResponse NormalizeWorkOrderBehavior(MaintenancePartsKitWorkOrderBehaviorRequest? request) =>
        new(
            request?.CanBeManuallyAdded ?? true,
            request?.AutoSuggestOnMatchingWorkOrder ?? false,
            request?.AutoAddToMatchingWorkOrder ?? false,
            request?.AutoAddToPmGeneratedWorkOrder ?? false,
            request?.AutoAddAfterFailedInspectionQuestion ?? false,
            request?.AutoAddAfterMatchingDefectType ?? false,
            request?.RequireSupervisorApprovalBeforeAdding ?? false,
            request?.RequirePartsReviewBeforeWorkCanStart ?? false,
            request?.RequireAvailabilityCheckBeforeScheduling ?? false,
            request?.AllowTechnicianAdjustQuantities ?? true,
            request?.RequireAdjustmentReason ?? false,
            request?.AllowTechnicianRemoveOptionalItems ?? true,
            request?.AllowTechnicianRemoveRequiredItems ?? false,
            request?.RequireReasonToRemoveRequiredItem ?? false,
            request?.SnapshotKitItemsOntoWorkOrder ?? true,
            request?.KeepLiveReferenceAfterWorkOrderCreation ?? false);

    private static MaintenancePartsKitComplianceResponse NormalizeCompliance(MaintenancePartsKitComplianceRequest? request) =>
        new(
            request?.ComplianceRelated ?? false,
            NormalizeList(request?.GoverningBodyKeys),
            NormalizeList(request?.CitationRefs),
            request?.SafetyCritical ?? false,
            request?.ReadinessSensitive ?? false,
            request?.MissingRequiredPartsBlockWorkStart ?? false,
            request?.MissingRequiredPartsBlockWorkCompletion ?? false,
            request?.RequireSupervisorApprovalForSubstitution ?? false,
            request?.RequireDocumentationForSubstitution ?? false,
            request?.RequireFinalInspectionAfterUse ?? false,
            NormalizeOptional(request?.LinkedInspectionTemplateId));

    private static MaintenancePartsKitApprovalResponse NormalizeApproval(MaintenancePartsKitApprovalRequest? request) =>
        new(
            request?.RequiresApprovalBeforeActivation ?? false,
            NormalizeOptional(request?.ApproverRoleKey),
            NormalizeOptional(request?.ApproverPersonId),
            request?.RetireReplacedKitAfterActivation ?? false,
            NormalizeOptional(request?.NotesForApprover));

    private async Task SyncLegacyLinesAsync(
        MaintenancePartsKit kit,
        IReadOnlyList<MaintenancePartsKitItemResponse> items,
        CancellationToken cancellationToken)
    {
        var existingLines = kit.Lines.ToList();
        if (existingLines.Count > 0)
        {
            db.RemoveRange(existingLines);
        }

        if (items.Count == 0)
        {
            return;
        }

        kit.Lines.Clear();

        foreach (var (item, index) in items.Select((item, index) => (item, index)))
        {
            db.Add(new MaintenancePartsKitLine
            {
                Id = Guid.NewGuid(),
                TenantId = kit.TenantId,
                MaintenancePartsKitId = kit.Id,
                ItemRef = ResolveOwnershipItemRef(item),
                ItemDescriptionSnapshot = item.ItemDescriptionSnapshot,
                Quantity = item.Quantity,
                UnitOfMeasure = item.UnitOfMeasure,
                Required = item.Required,
                SubstituteAllowed = item.SubstituteAllowed,
                SortOrder = index,
                CreatedAt = DateTimeOffset.UtcNow.AddMilliseconds(index),
                UpdatedAt = DateTimeOffset.UtcNow.AddMilliseconds(index)
            });
        }

        await Task.CompletedTask;
    }

    private async Task<string> GenerateCloneKitNumberAsync(
        Guid tenantId,
        string sourceKitNumber,
        CancellationToken cancellationToken)
    {
        var normalizedSource = NormalizeKey(sourceKitNumber, "Kit number");
        var suffixIndex = 1;
        while (true)
        {
            var candidate = suffixIndex == 1
                ? $"{normalizedSource}-copy"
                : $"{normalizedSource}-copy-{suffixIndex}";
            var exists = await db.Set<MaintenancePartsKit>()
                .AnyAsync(x => x.TenantId == tenantId && x.KitNumber == candidate, cancellationToken);
            if (!exists)
            {
                return candidate;
            }

            suffixIndex += 1;
        }
    }

    private static MaintenancePartsKitPreviewInput NormalizePreviewRequest(MaintenancePartsKitPreviewRequest request) =>
        new(
            NormalizeKey(request.KitNumber, "Kit number"),
            NormalizeName(request.Title, "Title"),
            NormalizeDescription(request.Description),
            NormalizeList(request.AssetTypeApplicability),
            NormalizeList(request.WorkOrderTypeApplicability),
            NormalizeOptional(request.PmPlanRef),
            NormalizeOptional(request.KitCategoryKey),
            NormalizeOptional(request.KitTypeKey),
            NormalizeOptional(request.PriorityKey),
            NormalizeOptional(request.OwningSiteRef),
            NormalizeOptional(request.OwningTeamRef),
            NormalizeOptional(request.OwnerPersonId),
            NormalizeOptional(request.OwnerRoleKey),
            NormalizeList(request.Tags),
            NormalizeDefinition(request.Definition),
            request.EffectiveAt,
            request.ExpiresAt,
            NormalizeOptional(request.CloneSourcePartsKitId),
            NormalizeOptional(request.SelectedAssetId));

    private static MaintenancePartsKitPreviewInput BuildPreviewInputFromEntity(
        MaintenancePartsKit entity,
        string? selectedAssetId = null) =>
        new(
            entity.KitNumber,
            entity.Title,
            entity.Description,
            entity.AssetTypeApplicability,
            entity.WorkOrderTypeApplicability,
            entity.PmPlanRef,
            entity.KitCategoryKey,
            entity.KitTypeKey,
            entity.PriorityKey,
            entity.OwningSiteRef,
            entity.OwningTeamRef,
            entity.OwnerPersonId,
            entity.OwnerRoleKey,
            entity.Tags,
            DeserializeDefinition(entity.DefinitionJson),
            entity.EffectiveAt,
            entity.ExpiresAt,
            entity.CloneSourcePartsKitId,
            NormalizeOptional(selectedAssetId));

    private async Task<MaintenancePartsKitAnalysisResult> AnalyzeDefinitionAsync(
        Guid tenantId,
        MaintenancePartsKitPreviewInput input,
        bool forActivation,
        CancellationToken cancellationToken)
    {
        var definition = input.Definition;
        var warnings = new List<string>();
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.KitNumber))
        {
            errors.Add("Kit number is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            errors.Add("Title is required.");
        }

        if (definition.Items.Count == 0)
        {
            warnings.Add("Add at least one kit item before activating this kit.");
        }

        foreach (var duplicate in definition.Items
            .GroupBy(item => ResolveOwnershipItemRef(item), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1))
        {
            errors.Add($"Duplicate kit part reference '{duplicate.Key}'.");
        }

        var missingSupplyarrPartIds = definition.Items
            .Where(item => !item.IsPlaceholder && string.IsNullOrWhiteSpace(item.SupplyarrPartId))
            .Select(item => item.ItemRef)
            .ToArray();
        if (missingSupplyarrPartIds.Length > 0)
        {
            errors.Add($"Missing Supplyarr part IDs for: {string.Join(", ", missingSupplyarrPartIds)}.");
        }

        var missingItemDescriptions = definition.Items
            .Where(item => string.IsNullOrWhiteSpace(item.ItemDescriptionSnapshot))
            .Select(item => item.ItemRef)
            .ToArray();
        if (missingItemDescriptions.Length > 0)
        {
            errors.Add($"Missing item descriptions for: {string.Join(", ", missingItemDescriptions)}.");
        }

        foreach (var item in definition.Items)
        {
            if (item.Quantity < 0 || (!item.IsPlaceholder && item.Quantity <= 0))
            {
                errors.Add($"Item '{item.ItemRef}' must have a quantity greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(item.UnitOfMeasure))
            {
                errors.Add($"Item '{item.ItemRef}' must include a unit of measure.");
            }
        }

        var compatibleAssets = await FindCompatibleAssetsAsync(tenantId, definition, cancellationToken);
        var sampleAssets = compatibleAssets.Take(5).ToArray();
        if (compatibleAssets.Count == 0)
        {
            warnings.Add("No compatible assets were found for the current scope.");
        }

        if (!string.IsNullOrWhiteSpace(input.SelectedAssetId)
            && !compatibleAssets.Any(asset => string.Equals(asset.AssetId.ToString("D"), input.SelectedAssetId, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add("The selected asset does not match the current scope.");
        }

        var previewItems = await BuildPreviewItemsAsync(tenantId, definition, cancellationToken);
        var unsupportedScopeWarnings = BuildInformationalScopeWarnings(definition.AssetScope);
        warnings.AddRange(unsupportedScopeWarnings);

        var validation = new MaintenancePartsKitValidationResponse(
            errors.Count == 0,
            errors,
            warnings,
            compatibleAssets.Count,
            sampleAssets.Length,
            definition.Items.Count(item => item.Required),
            definition.Items.Count(item => !item.Required),
            definition.Items.Count(item => string.Equals(item.Criticality, "critical", StringComparison.OrdinalIgnoreCase)),
            BuildValidationSummary(input, definition, compatibleAssets.Count, sampleAssets.Length),
            true,
            errors.Count == 0,
            definition.Approval.RequiresApprovalBeforeActivation && errors.Count == 0);

        var blockers = new List<string>(errors);
        var preview = new MaintenancePartsKitPreviewResponse(
            validation,
            sampleAssets,
            previewItems,
            warnings,
            blockers,
            BuildAssetScopeSummary(definition.AssetScope),
            BuildWorkOrderBehaviorSummary(definition.WorkOrderBehavior),
            BuildComplianceSummary(definition.Compliance),
            BuildApprovalSummary(definition.Approval),
            BuildAvailabilitySummary(definition.Availability));

        return new MaintenancePartsKitAnalysisResult(validation, preview);
    }

    private async Task<IReadOnlyList<MaintenancePartsKitPreviewItemResponse>> BuildPreviewItemsAsync(
        Guid tenantId,
        MaintenancePartsKitDefinitionResponse definition,
        CancellationToken cancellationToken)
    {
        var items = new List<MaintenancePartsKitPreviewItemResponse>();
        foreach (var item in definition.Items)
        {
            var calculatedQuantity = CalculateQuantity(item, definition.QuantityRules);
            var availabilityStatus = "unknown";
            var availabilityMessage = "Part availability is not available.";

            if (definition.Availability.Enabled && Guid.TryParse(item.SupplyarrPartId, out var partId))
            {
                try
                {
                    var readiness = await supplyReadinessClient.GetPartReadinessAsync(
                        tenantId,
                        partId,
                        calculatedQuantity,
                        cancellationToken);

                    if (readiness.Availability.QuantityAvailable >= calculatedQuantity)
                    {
                        availabilityStatus = "available";
                    }
                    else if (readiness.Availability.QuantityAvailable > 0)
                    {
                        availabilityStatus = "limited";
                    }
                    else
                    {
                        availabilityStatus = "unavailable";
                    }

                    availabilityMessage =
                        $"{readiness.Availability.QuantityAvailable:0.###} available, {readiness.Availability.QuantityReserved:0.###} reserved.";
                }
                catch (Exception ex) when (ex is StlApiException or HttpRequestException)
                {
                    availabilityStatus = "unknown";
                    availabilityMessage = "Supply readiness lookup failed for this item.";
                }
            }
            else if (!definition.Availability.Enabled)
            {
                availabilityStatus = "disabled";
                availabilityMessage = "Availability preview is disabled for this kit.";
            }

            items.Add(new MaintenancePartsKitPreviewItemResponse(
                item.ItemRef,
                item.ItemDescriptionSnapshot,
                item.Quantity,
                calculatedQuantity,
                item.UnitOfMeasure,
                item.Criticality,
                availabilityStatus,
                availabilityMessage,
                item.SupplyarrPartId,
                item.PartNumberSnapshot,
                item.Required,
                item.SubstituteAllowed,
                item.IsPlaceholder));
        }

        return items;
    }

    private async Task<IReadOnlyList<AssetSearchResponse>> FindCompatibleAssetsAsync(
        Guid tenantId,
        MaintenancePartsKitDefinitionResponse definition,
        CancellationToken cancellationToken)
    {
        var includedAssetIds = definition.AssetScope.IncludedAssetIds
            .Select(value => Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty)
            .Where(value => value != Guid.Empty)
            .ToArray();
        var excludedAssetIds = definition.AssetScope.ExcludedAssetIds
            .Select(value => Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty)
            .Where(value => value != Guid.Empty)
            .ToArray();

        var query = db.Assets
            .AsNoTracking()
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .Where(x => x.TenantId == tenantId);

        if (definition.AssetScope.AssetClassKeys.Count > 0)
        {
            query = query.Where(x => definition.AssetScope.AssetClassKeys.Contains(x.AssetType.AssetClass.ClassKey));
        }

        if (definition.AssetScope.AssetTypeKeys.Count > 0)
        {
            query = query.Where(x => definition.AssetScope.AssetTypeKeys.Contains(x.AssetType.TypeKey));
        }

        if (definition.AssetScope.AssetStatusKeys.Count > 0)
        {
            query = query.Where(x => definition.AssetScope.AssetStatusKeys.Contains(x.LifecycleStatus));
        }

        if (definition.AssetScope.SiteRefs.Count > 0)
        {
            query = query.Where(x => x.SiteRef != null && definition.AssetScope.SiteRefs.Contains(x.SiteRef));
        }

        if (definition.AssetScope.IncludedAssetIds.Count > 0)
        {
            query = query.Where(x => includedAssetIds.Contains(x.Id));
        }

        if (definition.AssetScope.ExcludedAssetIds.Count > 0)
        {
            query = query.Where(x => !excludedAssetIds.Contains(x.Id));
        }

        var assets = await query
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.Name)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (assets.Count == 0)
        {
            return [];
        }

        var assetIds = assets.Select(x => x.Id).ToArray();
        var openDefectCounts = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .GroupBy(x => x.AssetId)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var openWorkOrderCounts = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .GroupBy(x => x.AssetId)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var readinessStatuses = await db.AssetReadinessStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToDictionaryAsync(x => x.AssetId, x => x.ReadinessStatusKey, cancellationToken);

        return assets
            .Select(asset => new AssetSearchResponse(
                asset.Id,
                asset.AssetTypeId,
                asset.AssetType.TypeKey,
                asset.AssetType.Name,
                asset.AssetType.AssetClass.ClassKey,
                asset.AssetType.AssetClass.Name,
                asset.AssetTag,
                asset.Name,
                asset.Description,
                asset.LifecycleStatus,
                asset.SiteRef,
                asset.StaffarrSiteOrgUnitId,
                asset.StaffarrSiteNameSnapshot,
                openDefectCounts.GetValueOrDefault(asset.Id, 0),
                openWorkOrderCounts.GetValueOrDefault(asset.Id, 0),
                readinessStatuses.GetValueOrDefault(asset.Id, "unknown"),
                asset.CreatedAt,
                asset.UpdatedAt))
            .ToArray();
    }

    private static decimal CalculateQuantity(
        MaintenancePartsKitItemResponse item,
        IReadOnlyList<MaintenancePartsKitQuantityRuleResponse> rules)
    {
        var quantity = item.Quantity;
        var matchingRule = rules.FirstOrDefault(rule =>
            string.Equals(rule.AppliesToItemRef, item.ItemRef, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rule.AppliesToItemRef, "*", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rule.AppliesToItemRef, "all", StringComparison.OrdinalIgnoreCase));

        if (matchingRule is null)
        {
            return quantity;
        }

        quantity = Math.Max(matchingRule.BaseQuantity, quantity * Math.Max(matchingRule.Multiplier, 0));
        if (matchingRule.MinimumQuantity is not null)
        {
            quantity = Math.Max(quantity, matchingRule.MinimumQuantity.Value);
        }

        if (matchingRule.MaximumQuantity is not null)
        {
            quantity = Math.Min(quantity, matchingRule.MaximumQuantity.Value);
        }

        return matchingRule.RoundingBehavior.ToLowerInvariant() switch
        {
            "ceil" or "ceiling" => Math.Ceiling(quantity),
            "floor" => Math.Floor(quantity),
            "none" => quantity,
            _ => decimal.Round(quantity, 3, MidpointRounding.AwayFromZero),
        };
    }

    private static string BuildValidationSummary(
        MaintenancePartsKitPreviewInput input,
        MaintenancePartsKitDefinitionResponse definition,
        int compatibleAssetCount,
        int sampleAssetCount)
    {
        var itemCount = definition.Items.Count;
        return $"{input.KitNumber} has {itemCount} item{(itemCount == 1 ? string.Empty : "s")} and {compatibleAssetCount} compatible asset{(compatibleAssetCount == 1 ? string.Empty : "s")} in preview.";
    }

    private static string BuildAssetScopeSummary(MaintenancePartsKitAssetScopeResponse scope)
    {
        var parts = new List<string>();
        if (scope.AssetClassKeys.Count > 0)
        {
            parts.Add($"{scope.AssetClassKeys.Count} asset class{(scope.AssetClassKeys.Count == 1 ? string.Empty : "es")}");
        }
        if (scope.AssetTypeKeys.Count > 0)
        {
            parts.Add($"{scope.AssetTypeKeys.Count} asset type{(scope.AssetTypeKeys.Count == 1 ? string.Empty : "s")}");
        }
        if (scope.SiteRefs.Count > 0)
        {
            parts.Add($"{scope.SiteRefs.Count} site{(scope.SiteRefs.Count == 1 ? string.Empty : "s")}");
        }
        if (scope.AssetStatusKeys.Count > 0)
        {
            parts.Add($"{scope.AssetStatusKeys.Count} lifecycle status{(scope.AssetStatusKeys.Count == 1 ? string.Empty : "es")}");
        }
        return parts.Count == 0 ? "Applies to all assets." : $"Targets {string.Join(", ", parts)}.";
    }

    private static string BuildWorkOrderBehaviorSummary(MaintenancePartsKitWorkOrderBehaviorResponse behavior)
    {
        var parts = new List<string>();
        if (behavior.CanBeManuallyAdded)
        {
            parts.Add("manually addable");
        }
        if (behavior.AutoSuggestOnMatchingWorkOrder)
        {
            parts.Add("auto-suggested on matching work orders");
        }
        if (behavior.AutoAddToMatchingWorkOrder || behavior.AutoAddToPmGeneratedWorkOrder)
        {
            parts.Add("auto-added to generated work orders");
        }
        if (behavior.SnapshotKitItemsOntoWorkOrder)
        {
            parts.Add("snapshotted onto work orders");
        }
        return parts.Count == 0 ? "No work order behaviors configured." : string.Join("; ", parts) + ".";
    }

    private static string BuildComplianceSummary(MaintenancePartsKitComplianceResponse compliance)
    {
        var parts = new List<string>();
        if (compliance.ComplianceRelated)
        {
            parts.Add("compliance related");
        }
        if (compliance.SafetyCritical)
        {
            parts.Add("safety critical");
        }
        if (compliance.ReadinessSensitive)
        {
            parts.Add("readiness sensitive");
        }
        return parts.Count == 0 ? "No compliance flags set." : string.Join("; ", parts) + ".";
    }

    private static string BuildApprovalSummary(MaintenancePartsKitApprovalResponse approval)
    {
        if (!approval.RequiresApprovalBeforeActivation)
        {
            return "No approval gate before activation.";
        }

        var approver = !string.IsNullOrWhiteSpace(approval.ApproverRoleKey)
            ? approval.ApproverRoleKey
            : approval.ApproverPersonId;
        return string.IsNullOrWhiteSpace(approver)
            ? "Approval required before activation."
            : $"Approval required before activation by {approver}.";
    }

    private static string BuildAvailabilitySummary(MaintenancePartsKitAvailabilityResponse availability)
    {
        if (!availability.Enabled)
        {
            return "Availability preview is disabled.";
        }

        var parts = new List<string>();
        if (availability.ShowSiteAvailability)
        {
            parts.Add("site availability");
        }
        if (availability.ShowNearbyAvailability)
        {
            parts.Add("nearby availability");
        }
        if (availability.ShowOnOrder)
        {
            parts.Add("on-order quantities");
        }
        if (availability.ShowEstimatedLeadTime)
        {
            parts.Add("lead time");
        }
        return parts.Count == 0 ? "Availability preview enabled." : $"Availability preview includes {string.Join(", ", parts)}.";
    }

    private static IReadOnlyList<string> BuildInformationalScopeWarnings(MaintenancePartsKitAssetScopeResponse scope)
    {
        var warnings = new List<string>();
        if (scope.DepartmentRefs.Count > 0
            || scope.MakeKeys.Count > 0
            || scope.ModelKeys.Count > 0
            || !string.IsNullOrWhiteSpace(scope.YearFrom)
            || !string.IsNullOrWhiteSpace(scope.YearTo)
            || scope.FuelTypeKeys.Count > 0
            || scope.BodyTypeKeys.Count > 0
            || scope.ConfigurationKeys.Count > 0
            || scope.VariantFlags.Count > 0
            || scope.RequiredAttributes.Count > 0
            || scope.ExcludedAttributes.Count > 0)
        {
            warnings.Add("Some asset scope filters are informational in preview and are not fully enforced yet.");
        }

        return warnings;
    }

    private sealed record MaintenancePartsKitPreviewInput(
        string KitNumber,
        string Title,
        string? Description,
        IReadOnlyList<string> AssetTypeApplicability,
        IReadOnlyList<string> WorkOrderTypeApplicability,
        string? PmPlanRef,
        string? KitCategoryKey,
        string? KitTypeKey,
        string? PriorityKey,
        string? OwningSiteRef,
        string? OwningTeamRef,
        string? OwnerPersonId,
        string? OwnerRoleKey,
        IReadOnlyList<string> Tags,
        MaintenancePartsKitDefinitionResponse Definition,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? ExpiresAt,
        string? CloneSourcePartsKitId,
        string? SelectedAssetId);

    private sealed record MaintenancePartsKitAnalysisResult(
        MaintenancePartsKitValidationResponse Validation,
        MaintenancePartsKitPreviewResponse Preview);

    private static MaintenancePartsKitPreviewRequest BuildPreviewRequestFromEntity(
        MaintenancePartsKit entity,
        string? selectedAssetId = null)
    {
        var definition = DeserializeDefinition(entity.DefinitionJson);
        return new MaintenancePartsKitPreviewRequest(
            entity.KitNumber,
            entity.Title,
            entity.Description,
            entity.AssetTypeApplicability,
            entity.WorkOrderTypeApplicability,
            entity.PmPlanRef,
            entity.KitCategoryKey,
            entity.KitTypeKey,
            entity.PriorityKey,
            entity.OwningSiteRef,
            entity.OwningTeamRef,
            entity.OwnerPersonId,
            entity.OwnerRoleKey,
            entity.Tags,
            new MaintenancePartsKitDefinitionRequest(
                definition.ApplicabilityWorkOrderTypes,
                definition.ApplicabilityPmProgramRefs,
                definition.ApplicabilityInspectionTemplateRefs,
                definition.ApplicabilityDefectTypes,
                definition.ApplicabilityTaskTemplateRefs,
                definition.ApplicabilityRepairCategories,
                definition.WorkSourceCompatibilities,
                new MaintenancePartsKitAssetScopeRequest(
                    definition.AssetScope.AssetClassKeys,
                    definition.AssetScope.AssetTypeKeys,
                    definition.AssetScope.AssetCategoryKeys,
                    definition.AssetScope.AssetStatusKeys,
                    definition.AssetScope.SiteRefs,
                    definition.AssetScope.DepartmentRefs,
                    definition.AssetScope.MakeKeys,
                    definition.AssetScope.ModelKeys,
                    definition.AssetScope.YearFrom,
                    definition.AssetScope.YearTo,
                    definition.AssetScope.FuelTypeKeys,
                    definition.AssetScope.BodyTypeKeys,
                    definition.AssetScope.ConfigurationKeys,
                    definition.AssetScope.VariantFlags,
                    definition.AssetScope.RequiredAttributes,
                    definition.AssetScope.ExcludedAttributes,
                    definition.AssetScope.IncludedAssetIds,
                    definition.AssetScope.ExcludedAssetIds),
                definition.Items.Select(item => new MaintenancePartsKitItemRequest(
                    item.ItemRef,
                    item.SupplyarrPartId,
                    item.ItemDescriptionSnapshot,
                    item.PartNumberSnapshot,
                    item.ManufacturerPartNumberSnapshot,
                    item.VendorPartNumberSnapshot,
                    item.Quantity,
                    item.UnitOfMeasure,
                    item.Required,
                    item.Criticality,
                    item.SubstituteAllowed,
                    item.PreferredSubstituteRefs,
                    item.Consumable,
                    item.Serialized,
                    item.CoreReturnExpected,
                    item.Hazardous,
                    item.WarrantySensitive,
                    item.RequiredByTask,
                    item.Notes,
                    item.Tags,
                    item.IsPlaceholder)).ToArray(),
                definition.QuantityRules.Select(rule => new MaintenancePartsKitQuantityRuleRequest(
                    rule.RuleId,
                    rule.RuleType,
                    rule.AppliesToItemRef,
                    rule.AssetConditionSummary,
                    rule.WorkConditionSummary,
                    rule.ConditionSummary,
                    rule.BaseQuantity,
                    rule.Multiplier,
                    rule.MinimumQuantity,
                    rule.MaximumQuantity,
                    rule.RoundingBehavior,
                    rule.PlainLanguageSummary)).ToArray(),
                new MaintenancePartsKitAvailabilityRequest(
                    definition.Availability.Enabled,
                    definition.Availability.PreferredFulfillmentSource,
                    definition.Availability.ShowSiteAvailability,
                    definition.Availability.ShowNearbyAvailability,
                    definition.Availability.ShowOnOrder,
                    definition.Availability.ShowEstimatedLeadTime,
                    definition.Availability.RequestReservation,
                    definition.Availability.Notes),
                new MaintenancePartsKitWorkOrderBehaviorRequest(
                    definition.WorkOrderBehavior.CanBeManuallyAdded,
                    definition.WorkOrderBehavior.AutoSuggestOnMatchingWorkOrder,
                    definition.WorkOrderBehavior.AutoAddToMatchingWorkOrder,
                    definition.WorkOrderBehavior.AutoAddToPmGeneratedWorkOrder,
                    definition.WorkOrderBehavior.AutoAddAfterFailedInspectionQuestion,
                    definition.WorkOrderBehavior.AutoAddAfterMatchingDefectType,
                    definition.WorkOrderBehavior.RequireSupervisorApprovalBeforeAdding,
                    definition.WorkOrderBehavior.RequirePartsReviewBeforeWorkCanStart,
                    definition.WorkOrderBehavior.RequireAvailabilityCheckBeforeScheduling,
                    definition.WorkOrderBehavior.AllowTechnicianAdjustQuantities,
                    definition.WorkOrderBehavior.RequireAdjustmentReason,
                    definition.WorkOrderBehavior.AllowTechnicianRemoveOptionalItems,
                    definition.WorkOrderBehavior.AllowTechnicianRemoveRequiredItems,
                    definition.WorkOrderBehavior.RequireReasonToRemoveRequiredItem,
                    definition.WorkOrderBehavior.SnapshotKitItemsOntoWorkOrder,
                    definition.WorkOrderBehavior.KeepLiveReferenceAfterWorkOrderCreation),
                new MaintenancePartsKitComplianceRequest(
                    definition.Compliance.ComplianceRelated,
                    definition.Compliance.GoverningBodyKeys,
                    definition.Compliance.CitationRefs,
                    definition.Compliance.SafetyCritical,
                    definition.Compliance.ReadinessSensitive,
                    definition.Compliance.MissingRequiredPartsBlockWorkStart,
                    definition.Compliance.MissingRequiredPartsBlockWorkCompletion,
                    definition.Compliance.RequireSupervisorApprovalForSubstitution,
                    definition.Compliance.RequireDocumentationForSubstitution,
                    definition.Compliance.RequireFinalInspectionAfterUse,
                    definition.Compliance.LinkedInspectionTemplateId),
                new MaintenancePartsKitApprovalRequest(
                    definition.Approval.RequiresApprovalBeforeActivation,
                    definition.Approval.ApproverRoleKey,
                    definition.Approval.ApproverPersonId,
                    definition.Approval.RetireReplacedKitAfterActivation,
                    definition.Approval.NotesForApprover),
                definition.ChangeReason,
                definition.VersionLabel),
            entity.EffectiveAt,
            entity.ExpiresAt,
            entity.CloneSourcePartsKitId,
            null);
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!MaintenancePartsKitStatuses.All.Contains(normalized))
        {
            throw new StlApiException(
                "maintenance_parts_kits.validation",
                "Status must be draft, pending_approval, active, paused, retired, or archived.",
                400);
        }

        return normalized;
    }
}
