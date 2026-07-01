using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;
using Microsoft.Extensions.Options;

namespace SupplyArr.Api.Services;

public sealed class ComplianceCoreFactPublisherService(
    SupplyArrDbContext db,
    ComplianceCoreFactPublicationClient complianceCoreClient,
    IOptions<ComplianceCoreClientOptions> complianceCoreOptions)
{
    private static readonly HashSet<string> ProcurementOutboxKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        IntegrationOutboxEventKinds.PurchaseRequestSubmitted,
        IntegrationOutboxEventKinds.PurchaseRequestApproved,
        IntegrationOutboxEventKinds.PurchaseOrderIssued,
        IntegrationOutboxEventKinds.ReceivingReceiptPosted,
        IntegrationOutboxEventKinds.ReceivingExceptionCreated,
        IntegrationOutboxEventKinds.ReceivingExceptionResolved,
        IntegrationOutboxEventKinds.ReceivingExceptionCancelled,
        IntegrationOutboxEventKinds.ReceivingExceptionReopened,
        IntegrationOutboxEventKinds.SupplierOnboardingSubmitted,
        IntegrationOutboxEventKinds.SupplierOnboardingApproved,
        IntegrationOutboxEventKinds.SupplierOnboardingRejected,
        IntegrationOutboxEventKinds.SupplierOnboardingSuspended,
        IntegrationOutboxEventKinds.SupplierComplianceDocumentRegistered,
        IntegrationOutboxEventKinds.SupplierComplianceDocumentApproved,
        IntegrationOutboxEventKinds.SupplierComplianceDocumentRejected,
        IntegrationOutboxEventKinds.SupplierRestrictionCreated,
        IntegrationOutboxEventKinds.SupplierRestrictionUpdated,
        IntegrationOutboxEventKinds.SupplierRestrictionLifted,
        IntegrationOutboxEventKinds.ProcurementExceptionCreated,
        IntegrationOutboxEventKinds.ProcurementExceptionUpdated,
        IntegrationOutboxEventKinds.ProcurementExceptionInvestigating,
        IntegrationOutboxEventKinds.ProcurementExceptionResolved,
        IntegrationOutboxEventKinds.ProcurementExceptionWaiveRequested,
        IntegrationOutboxEventKinds.ProcurementExceptionWaived,
        IntegrationOutboxEventKinds.ProcurementExceptionWaiveRejected,
        IntegrationOutboxEventKinds.ProcurementExceptionClosed,
        IntegrationOutboxEventKinds.ProcurementExceptionCancelled,
        IntegrationOutboxEventKinds.ProcurementExceptionReopened,
        IntegrationOutboxEventKinds.EmergencyPurchaseCreated,
        IntegrationOutboxEventKinds.EmergencyPurchaseExpeditedSubmitted,
        IntegrationOutboxEventKinds.EmergencyPurchaseManagerOverrideApproved,
        IntegrationOutboxEventKinds.EmergencyPurchaseOrderIssued,
        IntegrationOutboxEventKinds.SupplierIncidentCreated,
        IntegrationOutboxEventKinds.SupplierIncidentUpdated,
        IntegrationOutboxEventKinds.SupplierIncidentInvestigating,
        IntegrationOutboxEventKinds.SupplierIncidentResolved,
        IntegrationOutboxEventKinds.SupplierIncidentClosed,
        IntegrationOutboxEventKinds.SupplierIncidentCancelled,
        IntegrationOutboxEventKinds.SupplierIncidentReopened,
        IntegrationOutboxEventKinds.SupplierIncidentRestrictionApplied,
        IntegrationOutboxEventKinds.WarrantyClaimCreated,
        IntegrationOutboxEventKinds.WarrantyClaimUpdated,
        IntegrationOutboxEventKinds.WarrantyClaimSubmitted,
        IntegrationOutboxEventKinds.WarrantyClaimSupplierResponded,
        IntegrationOutboxEventKinds.WarrantyClaimClosed,
        IntegrationOutboxEventKinds.WarrantyClaimDenied,
        IntegrationOutboxEventKinds.WarrantyClaimCancelled,
    };

    public static bool ShouldPublishForEventKind(string eventKind) =>
        ProcurementOutboxKinds.Contains(eventKind);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(complianceCoreOptions.Value.ServiceToken))
        {
            return;
        }

        if (!ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var facts = await BuildFactsAsync(outboxEvent, cancellationToken);
        if (facts.Count == 0)
        {
            return;
        }

        var publishedAt = outboxEvent.ProcessedAt ?? DateTimeOffset.UtcNow;
        await complianceCoreClient.IngestAsync(
            new ComplianceCoreIngestProductFactsPayload(
                outboxEvent.TenantId,
                outboxEvent.Id,
                "supplyarr",
                publishedAt,
                facts),
            cancellationToken);
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        return outboxEvent.EventKind switch
        {
            IntegrationOutboxEventKinds.PurchaseRequestSubmitted
            or IntegrationOutboxEventKinds.PurchaseRequestApproved
                => await BuildPurchaseRequestFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.PurchaseOrderIssued
                => await BuildPurchaseOrderFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.ReceivingReceiptPosted
                => await BuildReceivingFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.ReceivingExceptionCreated
            or IntegrationOutboxEventKinds.ReceivingExceptionResolved
            or IntegrationOutboxEventKinds.ReceivingExceptionCancelled
            or IntegrationOutboxEventKinds.ReceivingExceptionReopened
                => await BuildReceivingExceptionFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.SupplierOnboardingSubmitted
            or IntegrationOutboxEventKinds.SupplierOnboardingApproved
            or IntegrationOutboxEventKinds.SupplierOnboardingRejected
            or IntegrationOutboxEventKinds.SupplierOnboardingSuspended
                => await BuildSupplierOnboardingFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.SupplierComplianceDocumentRegistered
            or IntegrationOutboxEventKinds.SupplierComplianceDocumentApproved
            or IntegrationOutboxEventKinds.SupplierComplianceDocumentRejected
                => await BuildSupplierComplianceDocumentFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.SupplierRestrictionCreated
            or IntegrationOutboxEventKinds.SupplierRestrictionUpdated
            or IntegrationOutboxEventKinds.SupplierRestrictionLifted
                => await BuildSupplierRestrictionFactsAsync(outboxEvent, cancellationToken),
            _ when outboxEvent.EventKind.StartsWith("procurement_exception.", StringComparison.OrdinalIgnoreCase)
                => await BuildProcurementExceptionFactsAsync(outboxEvent, cancellationToken),
            _ when outboxEvent.EventKind.StartsWith("emergency_purchase.", StringComparison.OrdinalIgnoreCase)
                => await BuildEmergencyPurchaseFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.SupplierIncidentRestrictionApplied
                => await BuildSupplierIncidentRestrictionFactsAsync(outboxEvent, cancellationToken),
            _ when outboxEvent.EventKind.StartsWith("supplier_incident.", StringComparison.OrdinalIgnoreCase)
                => await BuildSupplierIncidentLifecycleFactsAsync(outboxEvent, cancellationToken),
            _ when outboxEvent.EventKind.StartsWith("warranty_claim.", StringComparison.OrdinalIgnoreCase)
                => await BuildWarrantyClaimFactsAsync(outboxEvent, cancellationToken),
            _ => Array.Empty<ComplianceCoreFactPublicationItem>(),
        };
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildPurchaseRequestFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.PurchaseRequests.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForPurchaseRequest(entity.Id);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.PurchaseRequestStatus,
                scopeKey,
                entity.Status,
                "purchase_request",
                entity.Id)
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildPurchaseOrderFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.PurchaseOrders.AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForPurchaseOrder(entity.Id);
        var facts = new List<ComplianceCoreFactPublicationItem>
        {
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.PurchaseOrderStatus,
                scopeKey,
                entity.Status,
                "purchase_order",
                entity.Id)
        };

        var supplierApprovalStatus = entity.Supplier.ApprovalStatus;
        var approvedSupplier = string.Equals(supplierApprovalStatus, "approved", StringComparison.OrdinalIgnoreCase);
        foreach (var line in entity.Lines.OrderBy(x => x.LineNumber))
        {
            var lineScopeKey = ScopeForPurchaseOrderLine(line.Id);
            facts.Add(StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.PurchaseOrderLineSupplierApprovalStatus,
                lineScopeKey,
                supplierApprovalStatus,
                "purchase_order_line",
                line.Id));
            facts.Add(BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.PartSourcedFromApprovedSupplier,
                lineScopeKey,
                approvedSupplier,
                "purchase_order_line",
                line.Id));
        }

        return facts;
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildReceivingFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.ReceivingReceipts.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForReceivingReceipt(entity.Id);
        var posted = ReceivingReceiptStatuses.IsPostedLike(entity.Status);
        return
        [
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.ReceivingReceiptPosted,
                scopeKey,
                posted,
                "receiving_receipt",
                entity.Id)
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildReceivingExceptionFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.ReceivingExceptions.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForReceivingException(entity.Id);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.ReceivingExceptionStatus,
                scopeKey,
                entity.Status,
                "receiving_exception",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.ReceivingDiscrepancyRecorded,
                scopeKey,
                true,
                "receiving_exception",
                entity.Id),
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildSupplierOnboardingFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierOnboardings.AsNoTracking()
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForSupplier(entity.SupplierId);
        var isApproved = string.Equals(
            entity.Supplier.ApprovalStatus,
            "approved",
            StringComparison.OrdinalIgnoreCase);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierApprovalStatus,
                scopeKey,
                entity.Supplier.ApprovalStatus,
                "supplier",
                entity.SupplierId),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierIsApproved,
                scopeKey,
                isApproved,
                "supplier",
                entity.SupplierId),
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildSupplierComplianceDocumentFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierComplianceDocuments.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForSupplierComplianceDocument(entity.Id);
        var expired = entity.ExpiresAt is DateTimeOffset expiresAt && expiresAt <= DateTimeOffset.UtcNow;
        var effectiveStatus = expired
            && string.Equals(entity.ReviewStatus, SupplierComplianceDocumentReviewStatuses.Approved, StringComparison.OrdinalIgnoreCase)
                ? SupplierComplianceDocumentReviewStatuses.Expired
                : entity.ReviewStatus;
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierDocumentStatus,
                scopeKey,
                effectiveStatus,
                "supplier_compliance_document",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierDocumentAttached,
                scopeKey,
                true,
                "supplier_compliance_document",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierDocumentExpired,
                scopeKey,
                expired,
                "supplier_compliance_document",
                entity.Id),
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildSupplierRestrictionFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierRestrictions.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForSupplier(entity.SupplierId);
        var blocks = string.Equals(entity.Status, SupplierRestrictionStatuses.Active, StringComparison.OrdinalIgnoreCase);
        return
        [
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierRestrictionBlocksProcurement,
                scopeKey,
                blocks,
                "supplier_restriction",
                entity.Id)
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildProcurementExceptionFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.ProcurementExceptions.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForProcurementException(entity.Id);
        var isActive = ProcurementExceptionStatuses.Active.Contains(entity.Status);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.ProcurementExceptionStatus,
                scopeKey,
                entity.Status,
                "procurement_exception",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.ProcurementExceptionIsActive,
                scopeKey,
                isActive,
                "procurement_exception",
                entity.Id),
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildEmergencyPurchaseFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await LoadEmergencyPurchaseRequestAsync(outboxEvent, cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForPurchaseRequest(entity.Id);
        var justified = !string.IsNullOrWhiteSpace(entity.EmergencyReason)
            && (entity.ManagerOverrideApproved || !string.IsNullOrWhiteSpace(entity.ManagerOverrideJustification));
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.EmergencyPurchaseStatus,
                scopeKey,
                entity.Status,
                "purchase_request",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.EmergencyPurchaseJustified,
                scopeKey,
                justified,
                "purchase_request",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.EmergencyPurchaseManagerOverrideApproved,
                scopeKey,
                entity.ManagerOverrideApproved,
                "purchase_request",
                entity.Id),
        ];
    }

    private async Task<PurchaseRequest?> LoadEmergencyPurchaseRequestAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        if (string.Equals(outboxEvent.RelatedEntityType, "purchase_order", StringComparison.OrdinalIgnoreCase))
        {
            var purchaseOrder = await db.PurchaseOrders.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                    cancellationToken);
            if (purchaseOrder is null)
            {
                return null;
            }

            return await db.PurchaseRequests.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == outboxEvent.TenantId
                        && x.Id == purchaseOrder.PurchaseRequestId
                        && x.IsEmergency,
                    cancellationToken);
        }

        return await db.PurchaseRequests.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId
                    && x.Id == outboxEvent.RelatedEntityId
                    && x.IsEmergency,
                cancellationToken);
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildSupplierIncidentLifecycleFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierIncidents.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForSupplierIncident(entity.Id);
        var isActive = SupplierIncidentStatuses.Active.Contains(entity.Status);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierIncidentStatus,
                scopeKey,
                entity.Status,
                "supplier_incident",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierIncidentIsActive,
                scopeKey,
                isActive,
                "supplier_incident",
                entity.Id),
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildSupplierIncidentRestrictionFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierIncidents.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var activeRestriction = await db.SupplierRestrictions.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == outboxEvent.TenantId
                    && x.SupplierId == entity.SupplierId
                    && x.Status == SupplierRestrictionStatuses.Active,
                cancellationToken);

        var scopeKey = ScopeForSupplier(entity.SupplierId);
        return
        [
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.SupplierRestrictionBlocksProcurement,
                scopeKey,
                activeRestriction,
                "supplier_incident",
                entity.Id)
        ];
    }

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildWarrantyClaimFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.WarrantyClaims.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForWarrantyClaim(entity.Id);
        var filed = string.Equals(entity.Status, WarrantyClaimStatuses.Submitted, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, WarrantyClaimStatuses.SupplierResponded, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, WarrantyClaimStatuses.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, WarrantyClaimStatuses.Denied, StringComparison.OrdinalIgnoreCase);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.WarrantyClaimStatus,
                scopeKey,
                entity.Status,
                "warranty_claim",
                entity.Id),
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.WarrantyClaimFiled,
                scopeKey,
                filed,
                "warranty_claim",
                entity.Id),
        ];
    }

    private static ComplianceCoreFactPublicationItem StringFact(
        IntegrationOutboxEvent outboxEvent,
        string factKey,
        string scopeKey,
        string value,
        string entityType,
        Guid entityId) =>
        new(
            factKey,
            "string",
            scopeKey,
            value,
            null,
            null,
            null,
            entityType,
            entityId,
            outboxEvent.EventKind,
            IdempotencyKey(outboxEvent, factKey, scopeKey));

    private static ComplianceCoreFactPublicationItem BooleanFact(
        IntegrationOutboxEvent outboxEvent,
        string factKey,
        string scopeKey,
        bool value,
        string entityType,
        Guid entityId) =>
        new(
            factKey,
            "boolean",
            scopeKey,
            null,
            value,
            null,
            null,
            entityType,
            entityId,
            outboxEvent.EventKind,
            IdempotencyKey(outboxEvent, factKey, scopeKey));

    private static string IdempotencyKey(IntegrationOutboxEvent outboxEvent, string factKey, string scopeKey) =>
        $"supplyarr:{outboxEvent.Id:D}:{factKey}:{scopeKey}".ToLowerInvariant();

    private static string ScopeForPurchaseRequest(Guid id) => $"purchase_request:{id:D}".ToLowerInvariant();

    private static string ScopeForPurchaseOrder(Guid id) => $"purchase_order:{id:D}".ToLowerInvariant();

    private static string ScopeForPurchaseOrderLine(Guid id) => $"purchase_order_line:{id:D}".ToLowerInvariant();

    private static string ScopeForReceivingReceipt(Guid id) => $"receiving_receipt:{id:D}".ToLowerInvariant();

    private static string ScopeForReceivingException(Guid id) => $"receiving_exception:{id:D}".ToLowerInvariant();

    private static string ScopeForSupplier(Guid supplierId) => $"supplier:{supplierId:D}".ToLowerInvariant();

    private static string ScopeForSupplierComplianceDocument(Guid id) =>
        $"supplier_document:{id:D}".ToLowerInvariant();

    private static string ScopeForProcurementException(Guid id) => $"procurement_exception:{id:D}".ToLowerInvariant();

    private static string ScopeForSupplierIncident(Guid id) => $"supplier_incident:{id:D}".ToLowerInvariant();

    private static string ScopeForWarrantyClaim(Guid id) => $"warranty_claim:{id:D}".ToLowerInvariant();
}

