using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class SupplyArrSmartImportCommitHandler(SupplyArrDbContext db) : ISmartImportDestinationCommitHandler
{
    public string ProductKey => "supplyarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "supplyarr.smart_import.operation_not_supported",
                "SupplyArr Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("part", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("item", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("material", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitPartAsync(request, cancellationToken);
        }

        if (entityType.Contains("purchase", StringComparison.OrdinalIgnoreCase)
            && entityType.Contains("request", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitPurchaseRequestAsync(request, cancellationToken);
        }

        if (entityType.Contains("vendor", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("supplier", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("party", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitExternalPartyAsync(request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "supplyarr.smart_import.entity_type_not_supported",
            $"SupplyArr does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitExternalPartyAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.ExternalParties.FirstOrDefaultAsync(
            party => party.TenantId == request.TenantId && party.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.DisplayName);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var displayName = SmartImportPayloadReader.DisplayName(payload, $"Imported supplier {shortId}");
        var partyKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "partyKey", "vendorKey", "supplierKey", "code", "vendorNumber", "supplierNumber")
            ?? displayName,
            $"si_party_{shortId}",
            128);
        var duplicate = await db.ExternalParties.FirstOrDefaultAsync(
            party => party.TenantId == request.TenantId && party.PartyKey == partyKey,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.DisplayName);
        }

        var now = DateTimeOffset.UtcNow;
        var partyEntity = new ExternalParty
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            PartyKey = partyKey,
            PartyType = NormalizePartyType(SmartImportPayloadReader.GetString(payload, "partyType", "type")),
            DisplayName = SmartImportPayloadReader.Truncate(displayName, 256),
            LegalName = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "legalName", "name") ?? displayName,
                256),
            TaxIdentifier = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "taxIdentifier", "taxId"),
                64),
            ApprovalStatus = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "approvalStatus") ?? "pending",
                32),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "active",
                32),
            Notes = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "notes", "description") ?? "Created by reviewed Smart Import commit.",
                1024),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.ExternalParties.Add(partyEntity);
        AddAudit(request, "smart_import.external_party_created", "external_party", partyEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(partyEntity.Id, partyEntity.DisplayName);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitPartAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.Parts.FirstOrDefaultAsync(
            part => part.TenantId == request.TenantId && part.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.DisplayName);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var displayName = SmartImportPayloadReader.DisplayName(payload, $"Imported part {shortId}");
        var partKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "partKey", "partNumber", "sku", "itemNumber", "manufacturerPartNumber")
            ?? displayName,
            $"si_part_{shortId}",
            128);
        var duplicate = await db.Parts.FirstOrDefaultAsync(
            part => part.TenantId == request.TenantId && part.PartKey == partKey,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.DisplayName);
        }

        var now = DateTimeOffset.UtcNow;
        var partEntity = new Part
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            PartKey = partKey,
            DisplayName = SmartImportPayloadReader.Truncate(displayName, 256),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes") ?? displayName,
                512),
            CategoryKey = SmartImportPayloadReader.SlugKey(
                SmartImportPayloadReader.GetString(payload, "categoryKey", "category"),
                "uncategorized",
                128),
            UnitOfMeasure = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "unitOfMeasure", "uom") ?? "each",
                64),
            ManufacturerName = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "manufacturerName", "manufacturer"),
                256),
            ManufacturerPartNumber = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "manufacturerPartNumber", "mpn"),
                128),
            Status = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "status") ?? "active",
                32),
            RequiresSerialLotTracking = SmartImportPayloadReader.GetBool(payload, false, "requiresSerialLotTracking", "serialized"),
            ReorderPoint = SmartImportPayloadReader.GetDecimal(payload, "reorderPoint"),
            ReorderQuantity = SmartImportPayloadReader.GetDecimal(payload, "reorderQuantity"),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Parts.Add(partEntity);
        AddAudit(request, "smart_import.part_created", "part", partEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(partEntity.Id, partEntity.DisplayName);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitPurchaseRequestAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.PurchaseRequests.FirstOrDefaultAsync(
            purchaseRequest => purchaseRequest.TenantId == request.TenantId && purchaseRequest.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Title);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var requestKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "requestKey", "purchaseRequestNumber", "prNumber"),
            $"si_pr_{shortId}",
            64);
        var duplicate = await db.PurchaseRequests.FirstOrDefaultAsync(
            purchaseRequest => purchaseRequest.TenantId == request.TenantId && purchaseRequest.RequestKey == requestKey,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Title);
        }

        var now = DateTimeOffset.UtcNow;
        var title = SmartImportPayloadReader.DisplayName(payload, $"Imported purchase request {shortId}");
        var purchaseRequestEntity = new PurchaseRequest
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            RequestKey = requestKey,
            Title = SmartImportPayloadReader.Truncate(title, 256),
            Notes = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "notes", "description") ?? "Created by reviewed Smart Import commit.",
                1024),
            Status = PurchaseRequestStatuses.Draft,
            RequestedByUserId = request.ApprovedByPersonId,
            IsEmergency = SmartImportPayloadReader.GetBool(payload, false, "isEmergency", "emergency"),
            EmergencyReason = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "emergencyReason"),
                512),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PurchaseRequests.Add(purchaseRequestEntity);
        AddAudit(request, "smart_import.purchase_request_created", "purchase_request", purchaseRequestEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(purchaseRequestEntity.Id, purchaseRequestEntity.Title);
    }

    private void AddAudit(
        SmartImportDestinationCommitRequest request,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
    {
        db.AuditEvents.Add(new SupplyArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorUserId = request.ApprovedByPersonId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = "success",
            ReasonCode = "smart_import",
            CorrelationId = request.CommitPlanId,
            OccurredAt = occurredAt
        });
    }

    private static string NormalizePartyType(string? partyType) =>
        string.IsNullOrWhiteSpace(partyType) ? "vendor" : SmartImportPayloadReader.Truncate(partyType.ToLowerInvariant(), 32);

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);
}
