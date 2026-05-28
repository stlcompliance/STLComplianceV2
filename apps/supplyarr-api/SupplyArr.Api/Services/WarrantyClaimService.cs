using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class WarrantyClaimService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<WarrantyClaimResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? vendorPartyId = null,
        Guid? partId = null,
        Guid? purchaseOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.WarrantyClaims
            .AsNoTracking()
            .Include(x => x.VendorParty)
            .Include(x => x.Part)
            .Include(x => x.PurchaseOrder)
            .Include(x => x.ReceivingReceipt)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        if (vendorPartyId is not null)
        {
            query = query.Where(x => x.VendorPartyId == vendorPartyId);
        }

        if (partId is not null)
        {
            query = query.Where(x => x.PartId == partId);
        }

        if (purchaseOrderId is not null)
        {
            query = query.Where(x => x.PurchaseOrderId == purchaseOrderId);
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<WarrantyClaimResponse> GetAsync(
        Guid tenantId,
        Guid warrantyClaimId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, warrantyClaimId, cancellationToken);
        return Map(entity);
    }

    public async Task<WarrantyClaimResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateWarrantyClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureVendorPartyAsync(tenantId, request.VendorPartyId, cancellationToken);
        await EnsurePartAsync(tenantId, request.PartId, cancellationToken);
        await ValidateRelatedEntitiesAsync(tenantId, request, cancellationToken);

        var claimKey = WarrantyClaimRules.NormalizeClaimKey(request.ClaimKey);
        var duplicateKey = await db.WarrantyClaims.AnyAsync(
            x => x.TenantId == tenantId && x.ClaimKey == claimKey,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "warranty_claims.duplicate_key",
                "A warranty claim with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new WarrantyClaim
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClaimKey = claimKey,
            Status = WarrantyClaimStatuses.Draft,
            ClaimType = WarrantyClaimRules.NormalizeClaimType(request.ClaimType),
            VendorPartyId = request.VendorPartyId,
            PartId = request.PartId,
            PurchaseOrderId = request.PurchaseOrderId,
            PurchaseOrderLineId = request.PurchaseOrderLineId,
            ReceivingReceiptId = request.ReceivingReceiptId,
            ReceivingReceiptLineId = request.ReceivingReceiptLineId,
            QuantityClaimed = WarrantyClaimRules.NormalizeQuantity(request.QuantityClaimed),
            ProblemDescription = WarrantyClaimRules.NormalizeProblemDescription(request.ProblemDescription),
            VendorRmaNumber = WarrantyClaimRules.NormalizeOptionalText(request.VendorRmaNumber, 128, "Vendor RMA number"),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WarrantyClaims.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.create",
            IntegrationOutboxEventKinds.WarrantyClaimCreated,
            tenantId,
            actorUserId,
            entity,
            $"Warranty claim drafted: {entity.ClaimKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<WarrantyClaimResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid warrantyClaimId,
        UpdateWarrantyClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, warrantyClaimId, cancellationToken);
        WarrantyClaimRules.EnsureStatus(entity, WarrantyClaimStatuses.Draft);

        await ValidateRelatedEntitiesAsync(
            tenantId,
            entity.VendorPartyId,
            entity.PartId,
            request.PurchaseOrderId,
            request.PurchaseOrderLineId,
            request.ReceivingReceiptId,
            request.ReceivingReceiptLineId,
            cancellationToken);

        entity.ClaimType = WarrantyClaimRules.NormalizeClaimType(request.ClaimType);
        entity.QuantityClaimed = WarrantyClaimRules.NormalizeQuantity(request.QuantityClaimed);
        entity.ProblemDescription = WarrantyClaimRules.NormalizeProblemDescription(request.ProblemDescription);
        entity.PurchaseOrderId = request.PurchaseOrderId;
        entity.PurchaseOrderLineId = request.PurchaseOrderLineId;
        entity.ReceivingReceiptId = request.ReceivingReceiptId;
        entity.ReceivingReceiptLineId = request.ReceivingReceiptLineId;
        entity.VendorRmaNumber = WarrantyClaimRules.NormalizeOptionalText(request.VendorRmaNumber, 128, "Vendor RMA number");
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.update",
            IntegrationOutboxEventKinds.WarrantyClaimUpdated,
            tenantId,
            actorUserId,
            entity,
            $"Warranty claim updated: {entity.ClaimKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<WarrantyClaimResponse> SubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid warrantyClaimId,
        SubmitWarrantyClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, warrantyClaimId, cancellationToken);
        WarrantyClaimRules.EnsureStatus(entity, WarrantyClaimStatuses.Draft);

        var now = DateTimeOffset.UtcNow;
        WarrantyClaimRules.Transition(entity, WarrantyClaimStatuses.Submitted);
        entity.SubmittedByUserId = actorUserId;
        entity.SubmittedAt = now;
        entity.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var notes = WarrantyClaimRules.NormalizeOptionalText(request.Notes, 512, "Submit notes");
            entity.ProblemDescription = $"{entity.ProblemDescription}\n\nSubmission: {notes}";
        }

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.submit",
            IntegrationOutboxEventKinds.WarrantyClaimSubmitted,
            tenantId,
            actorUserId,
            entity,
            $"Warranty claim submitted to vendor: {entity.ClaimKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<WarrantyClaimResponse> RecordVendorResponseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid warrantyClaimId,
        RecordWarrantyClaimVendorResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, warrantyClaimId, cancellationToken);
        WarrantyClaimRules.EnsureStatus(entity, WarrantyClaimStatuses.Submitted);

        var disposition = WarrantyClaimRules.NormalizeVendorDisposition(request.VendorDisposition);
        var now = DateTimeOffset.UtcNow;

        WarrantyClaimRules.Transition(entity, WarrantyClaimStatuses.VendorResponded);
        entity.VendorDisposition = disposition;
        entity.VendorResponseNotes = WarrantyClaimRules.NormalizeOptionalText(
            request.VendorResponseNotes,
            2048,
            "Vendor response notes");
        if (!string.IsNullOrWhiteSpace(request.VendorRmaNumber))
        {
            entity.VendorRmaNumber = WarrantyClaimRules.NormalizeOptionalText(
                request.VendorRmaNumber,
                128,
                "Vendor RMA number");
        }

        entity.VendorRespondedByUserId = actorUserId;
        entity.VendorRespondedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.vendor_response",
            IntegrationOutboxEventKinds.WarrantyClaimVendorResponded,
            tenantId,
            actorUserId,
            entity,
            $"Vendor response recorded for warranty claim {entity.ClaimKey}: {disposition}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<WarrantyClaimResponse> CloseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid warrantyClaimId,
        CloseWarrantyClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, warrantyClaimId, cancellationToken);
        WarrantyClaimRules.EnsureStatus(entity, WarrantyClaimStatuses.VendorResponded);

        if (string.Equals(entity.VendorDisposition, WarrantyClaimVendorDispositions.Denied, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "warranty_claims.vendor_denied",
                "Use deny when the vendor disposition is denied.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        WarrantyClaimRules.Transition(entity, WarrantyClaimStatuses.Closed);
        entity.ClosureNotes = WarrantyClaimRules.NormalizeOptionalText(request.ClosureNotes, 2048, "Closure notes");
        if (entity.ClosureNotes.Length < 3)
        {
            throw new StlApiException(
                "warranty_claims.invalid_closure_notes",
                "Closure notes must be at least 3 characters.",
                400);
        }

        entity.ClosedByUserId = actorUserId;
        entity.ClosedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.close",
            IntegrationOutboxEventKinds.WarrantyClaimClosed,
            tenantId,
            actorUserId,
            entity,
            $"Warranty claim closed: {entity.ClaimKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<WarrantyClaimResponse> DenyAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid warrantyClaimId,
        DenyWarrantyClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, warrantyClaimId, cancellationToken);
        if (entity.Status is not WarrantyClaimStatuses.Submitted and not WarrantyClaimStatuses.VendorResponded)
        {
            throw new StlApiException(
                "warranty_claims.invalid_status",
                "Warranty claim can only be denied while submitted or after vendor response.",
                409);
        }

        var denialReason = WarrantyClaimRules.NormalizeOptionalText(request.DenialReason, 2048, "Denial reason");
        if (denialReason.Length < 3)
        {
            throw new StlApiException(
                "warranty_claims.invalid_denial_reason",
                "Denial reason must be at least 3 characters.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        WarrantyClaimRules.Transition(entity, WarrantyClaimStatuses.Denied);
        entity.DenialReason = denialReason;
        entity.DeniedByUserId = actorUserId;
        entity.DeniedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.deny",
            IntegrationOutboxEventKinds.WarrantyClaimDenied,
            tenantId,
            actorUserId,
            entity,
            $"Warranty claim denied: {entity.ClaimKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<WarrantyClaimResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid warrantyClaimId,
        CancelWarrantyClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, warrantyClaimId, cancellationToken);
        WarrantyClaimRules.EnsureStatus(entity, WarrantyClaimStatuses.Draft, WarrantyClaimStatuses.Submitted);

        var reason = WarrantyClaimRules.NormalizeOptionalText(request.Reason, 512, "Cancellation reason");
        if (reason.Length < 3)
        {
            throw new StlApiException(
                "warranty_claims.invalid_cancel_reason",
                "Cancellation reason must be at least 3 characters.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        WarrantyClaimRules.Transition(entity, WarrantyClaimStatuses.Cancelled);
        entity.CancellationReason = reason;
        entity.CancelledByUserId = actorUserId;
        entity.CancelledAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "warranty_claim.cancel",
            IntegrationOutboxEventKinds.WarrantyClaimCancelled,
            tenantId,
            actorUserId,
            entity,
            $"Warranty claim cancelled: {entity.ClaimKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    private async Task ValidateRelatedEntitiesAsync(
        Guid tenantId,
        CreateWarrantyClaimRequest request,
        CancellationToken cancellationToken) =>
        await ValidateRelatedEntitiesAsync(
            tenantId,
            request.VendorPartyId,
            request.PartId,
            request.PurchaseOrderId,
            request.PurchaseOrderLineId,
            request.ReceivingReceiptId,
            request.ReceivingReceiptLineId,
            cancellationToken);

    private async Task ValidateRelatedEntitiesAsync(
        Guid tenantId,
        Guid vendorPartyId,
        Guid partId,
        Guid? purchaseOrderId,
        Guid? purchaseOrderLineId,
        Guid? receivingReceiptId,
        Guid? receivingReceiptLineId,
        CancellationToken cancellationToken)
    {
        if (purchaseOrderId is Guid poId)
        {
            var po = await db.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == poId, cancellationToken)
                ?? throw new StlApiException("warranty_claims.purchase_order_not_found", "Purchase order was not found.", 404);

            if (po.VendorPartyId != vendorPartyId)
            {
                throw new StlApiException(
                    "warranty_claims.purchase_order_vendor_mismatch",
                    "Purchase order vendor does not match the claim vendor.",
                    400);
            }
        }

        if (purchaseOrderLineId is Guid poLineId)
        {
            var line = await db.PurchaseOrderLines
                .AsNoTracking()
                .Include(x => x.PurchaseOrder)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == poLineId, cancellationToken)
                ?? throw new StlApiException("warranty_claims.purchase_order_line_not_found", "Purchase order line was not found.", 404);

            if (line.PartId != partId)
            {
                throw new StlApiException(
                    "warranty_claims.purchase_order_line_part_mismatch",
                    "Purchase order line part does not match the claim part.",
                    400);
            }

            if (purchaseOrderId is Guid expectedPoId && line.PurchaseOrderId != expectedPoId)
            {
                throw new StlApiException(
                    "warranty_claims.purchase_order_line_mismatch",
                    "Purchase order line does not belong to the linked purchase order.",
                    400);
            }
        }

        if (receivingReceiptId is Guid receiptId)
        {
            if (!await db.ReceivingReceipts.AnyAsync(x => x.TenantId == tenantId && x.Id == receiptId, cancellationToken))
            {
                throw new StlApiException("warranty_claims.receiving_receipt_not_found", "Receiving receipt was not found.", 404);
            }
        }

        if (receivingReceiptLineId is Guid receiptLineId)
        {
            var receiptLine = await db.ReceivingReceiptLines
                .AsNoTracking()
                .Include(x => x.ReceivingReceipt)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == receiptLineId, cancellationToken)
                ?? throw new StlApiException("warranty_claims.receiving_receipt_line_not_found", "Receiving receipt line was not found.", 404);

            if (receiptLine.PartId != partId)
            {
                throw new StlApiException(
                    "warranty_claims.receiving_line_part_mismatch",
                    "Receiving line part does not match the claim part.",
                    400);
            }

            if (receivingReceiptId is Guid expectedReceiptId && receiptLine.ReceivingReceiptId != expectedReceiptId)
            {
                throw new StlApiException(
                    "warranty_claims.receiving_line_mismatch",
                    "Receiving line does not belong to the linked receiving receipt.",
                    400);
            }
        }
    }

    private async Task EnsureVendorPartyAsync(
        Guid tenantId,
        Guid vendorPartyId,
        CancellationToken cancellationToken)
    {
        var party = await db.ExternalParties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == vendorPartyId, cancellationToken)
            ?? throw new StlApiException("warranty_claims.party_not_found", "Vendor party was not found.", 404);

        if (!VendorRestrictionPartyTypes.Allowed.Contains(party.PartyType))
        {
            throw new StlApiException(
                "warranty_claims.party_type_not_allowed",
                "Warranty claims apply only to vendor or supplier parties.",
                400);
        }
    }

    private async Task EnsurePartAsync(Guid tenantId, Guid partId, CancellationToken cancellationToken)
    {
        if (!await db.Parts.AnyAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken))
        {
            throw new StlApiException("warranty_claims.part_not_found", "Part was not found.", 404);
        }
    }

    private async Task WriteAuditAndOutboxAsync(
        string auditAction,
        string outboxKind,
        Guid tenantId,
        Guid actorUserId,
        WarrantyClaim entity,
        string summary,
        CancellationToken cancellationToken)
    {
        await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "warranty_claim",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            outboxKind,
            "warranty_claim",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, summary, entity.VendorPartyId),
            cancellationToken: cancellationToken);
    }

    private async Task<WarrantyClaim> LoadAsync(
        Guid tenantId,
        Guid warrantyClaimId,
        CancellationToken cancellationToken) =>
        await db.WarrantyClaims
            .AsNoTracking()
            .Include(x => x.VendorParty)
            .Include(x => x.Part)
            .Include(x => x.PurchaseOrder)
            .Include(x => x.ReceivingReceipt)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == warrantyClaimId, cancellationToken)
            ?? throw new StlApiException("warranty_claims.not_found", "Warranty claim was not found.", 404);

    private async Task<WarrantyClaim> LoadTrackedAsync(
        Guid tenantId,
        Guid warrantyClaimId,
        CancellationToken cancellationToken) =>
        await db.WarrantyClaims
            .Include(x => x.VendorParty)
            .Include(x => x.Part)
            .Include(x => x.PurchaseOrder)
            .Include(x => x.ReceivingReceipt)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == warrantyClaimId, cancellationToken)
            ?? throw new StlApiException("warranty_claims.not_found", "Warranty claim was not found.", 404);

    private static WarrantyClaimResponse Map(WarrantyClaim entity) =>
        new(
            entity.Id,
            entity.ClaimKey,
            entity.Status,
            entity.ClaimType,
            entity.VendorPartyId,
            entity.VendorParty.PartyKey,
            entity.VendorParty.DisplayName,
            entity.PartId,
            entity.Part.PartKey,
            entity.Part.DisplayName,
            entity.PurchaseOrderId,
            entity.PurchaseOrder?.OrderKey,
            entity.PurchaseOrderLineId,
            entity.ReceivingReceiptId,
            entity.ReceivingReceipt?.ReceiptKey,
            entity.ReceivingReceiptLineId,
            entity.QuantityClaimed,
            entity.ProblemDescription,
            entity.VendorRmaNumber,
            entity.VendorDisposition,
            entity.VendorResponseNotes,
            entity.ClosureNotes,
            entity.DenialReason,
            entity.CreatedByUserId,
            entity.SubmittedByUserId,
            entity.SubmittedAt,
            entity.VendorRespondedByUserId,
            entity.VendorRespondedAt,
            entity.ClosedByUserId,
            entity.ClosedAt,
            entity.DeniedByUserId,
            entity.DeniedAt,
            entity.CancellationReason,
            entity.CreatedAt,
            entity.UpdatedAt);
}
