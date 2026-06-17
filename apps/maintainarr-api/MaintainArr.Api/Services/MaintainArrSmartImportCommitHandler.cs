using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrSmartImportCommitHandler(MaintainArrDbContext db) : ISmartImportDestinationCommitHandler
{
    public string ProductKey => "maintainarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "maintainarr.smart_import.operation_not_supported",
                "MaintainArr Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("work", StringComparison.OrdinalIgnoreCase)
            && entityType.Contains("order", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitWorkOrderAsync(request, cancellationToken);
        }

        if (entityType.Contains("asset", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("vehicle", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("equipment", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitAssetAsync(request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "maintainarr.smart_import.entity_type_not_supported",
            $"MaintainArr does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitAssetAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.Assets.FirstOrDefaultAsync(
            asset => asset.TenantId == request.TenantId && asset.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Name);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var assetTag = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "assetTag", "assetNumber", "unitNumber", "vin"),
            $"SI-ASSET-{shortId}");
        var duplicate = await db.Assets.FirstOrDefaultAsync(
            asset => asset.TenantId == request.TenantId && asset.AssetTag == assetTag,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Name);
        }

        var assetType = await ResolveOrCreateAssetTypeAsync(request.TenantId, payload, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var name = SmartImportPayloadReader.DisplayName(payload, assetTag);
        var assetEntity = new Asset
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            AssetTypeId = assetType.Id,
            AssetTag = SmartImportPayloadReader.Truncate(assetTag, 64),
            Name = SmartImportPayloadReader.Truncate(name, 128),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes"),
                512),
            LifecycleStatus = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "lifecycleStatus", "status") ?? "active",
                32),
            SiteRef = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "siteRef", "locationRef", "siteId"),
                128),
            StaffarrSiteNameSnapshot = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "siteName", "locationName"),
                256),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Assets.Add(assetEntity);
        AddAudit(request, "smart_import.asset_created", "asset", assetEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(assetEntity.Id, assetEntity.Name);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitWorkOrderAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.WorkOrders.FirstOrDefaultAsync(
            workOrder => workOrder.TenantId == request.TenantId && workOrder.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Title);
        }

        var payload = request.DeterministicPayload;
        var asset = await ResolveExistingAssetAsync(request.TenantId, payload, cancellationToken);
        if (asset is null)
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "maintainarr.smart_import.asset_reference_required",
                "MaintainArr work order imports require an existing assetId or assetTag in the approved payload.");
        }

        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var workOrderNumber = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "workOrderNumber", "orderNumber", "ticketNumber"),
            $"SI-WO-{shortId}");
        var duplicate = await db.WorkOrders.FirstOrDefaultAsync(
            workOrder => workOrder.TenantId == request.TenantId && workOrder.WorkOrderNumber == workOrderNumber,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Title);
        }

        var now = DateTimeOffset.UtcNow;
        var title = SmartImportPayloadReader.DisplayName(payload, $"Imported work order {shortId}");
        var workOrderEntity = new WorkOrder
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            AssetId = asset.Id,
            WorkOrderNumber = SmartImportPayloadReader.Truncate(workOrderNumber, 64),
            Title = SmartImportPayloadReader.Truncate(title, 128),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes", "scope"),
                512),
            Priority = NormalizePriority(SmartImportPayloadReader.GetString(payload, "priority")),
            Status = WorkOrderStatuses.Draft,
            Source = WorkOrderSources.Manual,
            WorkOrderType = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "workOrderType", "type") ?? WorkOrderTypes.Corrective,
                64),
            OriginType = WorkOrderOriginTypes.Manual,
            OriginRef = request.RecordArrSourceRecordId,
            CreatedByUserId = request.ApprovedByPersonId,
            PlannedStartAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "plannedStartAt", "startDate"),
            PlannedDueAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "plannedDueAt", "dueDate"),
            AssignedTechnicianPersonId = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "assignedTechnicianPersonId", "technicianPersonId"),
                128),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.WorkOrders.Add(workOrderEntity);
        AddAudit(request, "smart_import.work_order_created", "work_order", workOrderEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(workOrderEntity.Id, workOrderEntity.Title);
    }

    private async Task<AssetType> ResolveOrCreateAssetTypeAsync(
        Guid tenantId,
        System.Text.Json.JsonElement payload,
        CancellationToken cancellationToken)
    {
        var classKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "assetClassKey", "assetClass", "class"),
            "smart_import",
            128);
        var assetClass = await db.AssetClasses.FirstOrDefaultAsync(
            candidate => candidate.TenantId == tenantId && candidate.ClassKey == classKey,
            cancellationToken);
        if (assetClass is null)
        {
            var now = DateTimeOffset.UtcNow;
            assetClass = new AssetClass
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClassKey = classKey,
                Name = SmartImportPayloadReader.Truncate(
                    SmartImportPayloadReader.GetString(payload, "assetClassName", "assetClass") ?? "Smart Import",
                    128),
                Description = "Created by reviewed Smart Import commit.",
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            };
            db.AssetClasses.Add(assetClass);
        }

        var typeKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "assetTypeKey", "assetType", "type"),
            "smart_import_asset",
            128);
        var assetType = await db.AssetTypes.FirstOrDefaultAsync(
            candidate => candidate.TenantId == tenantId && candidate.TypeKey == typeKey,
            cancellationToken);
        if (assetType is not null)
        {
            return assetType;
        }

        var createdAt = DateTimeOffset.UtcNow;
        assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetClassId = assetClass.Id,
            TypeKey = typeKey,
            Name = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "assetTypeName", "assetType", "type") ?? "Smart Import Asset",
                128),
            Description = "Created by reviewed Smart Import commit.",
            Status = "active",
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
        db.AssetTypes.Add(assetType);
        return assetType;
    }

    private async Task<Asset?> ResolveExistingAssetAsync(
        Guid tenantId,
        System.Text.Json.JsonElement payload,
        CancellationToken cancellationToken)
    {
        var assetId = SmartImportPayloadReader.GetGuid(payload, "assetId", "maintainarrAssetId");
        if (assetId is not null)
        {
            return await db.Assets.FirstOrDefaultAsync(
                asset => asset.TenantId == tenantId && asset.Id == assetId.Value,
                cancellationToken);
        }

        var assetTag = SmartImportPayloadReader.GetString(payload, "assetTag", "assetNumber", "unitNumber", "vin");
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            return null;
        }

        return await db.Assets.FirstOrDefaultAsync(
            asset => asset.TenantId == tenantId && asset.AssetTag == assetTag,
            cancellationToken);
    }

    private void AddAudit(
        SmartImportDestinationCommitRequest request,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
    {
        db.AuditEvents.Add(new MaintainArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorUserId = request.ApprovedByPersonId,
            ActorPersonId = request.ApprovedByPersonId.ToString("D"),
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = "success",
            ReasonCode = "smart_import",
            CorrelationId = request.CommitPlanId,
            OccurredAt = occurredAt
        });
    }

    private static string NormalizePriority(string? priority) =>
        WorkOrderPriorities.All.Contains(priority ?? string.Empty) ? priority!.ToLowerInvariant() : WorkOrderPriorities.Medium;

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);
}
