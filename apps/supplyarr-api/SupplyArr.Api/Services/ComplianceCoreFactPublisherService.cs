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
        IntegrationOutboxEventKinds.VendorRestrictionCreated,
        IntegrationOutboxEventKinds.VendorRestrictionUpdated,
        IntegrationOutboxEventKinds.VendorRestrictionLifted,
        IntegrationOutboxEventKinds.ProcurementExceptionCreated,
        IntegrationOutboxEventKinds.ProcurementExceptionUpdated,
        IntegrationOutboxEventKinds.ProcurementExceptionInvestigating,
        IntegrationOutboxEventKinds.ProcurementExceptionResolved,
        IntegrationOutboxEventKinds.ProcurementExceptionWaiveRequested,
        IntegrationOutboxEventKinds.ProcurementExceptionWaived,
        IntegrationOutboxEventKinds.ProcurementExceptionWaiveRejected,
        IntegrationOutboxEventKinds.ProcurementExceptionClosed,
        IntegrationOutboxEventKinds.ProcurementExceptionCancelled,
        IntegrationOutboxEventKinds.SupplierIncidentRestrictionApplied,
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
            IntegrationOutboxEventKinds.VendorRestrictionCreated
            or IntegrationOutboxEventKinds.VendorRestrictionUpdated
            or IntegrationOutboxEventKinds.VendorRestrictionLifted
                => await BuildVendorRestrictionFactsAsync(outboxEvent, cancellationToken),
            _ when outboxEvent.EventKind.StartsWith("procurement_exception.", StringComparison.OrdinalIgnoreCase)
                => await BuildProcurementExceptionFactsAsync(outboxEvent, cancellationToken),
            IntegrationOutboxEventKinds.SupplierIncidentRestrictionApplied
                => await BuildSupplierIncidentRestrictionFactsAsync(outboxEvent, cancellationToken),
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
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForPurchaseOrder(entity.Id);
        return
        [
            StringFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.PurchaseOrderStatus,
                scopeKey,
                entity.Status,
                "purchase_order",
                entity.Id)
        ];
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
        var posted = string.Equals(entity.Status, ReceivingReceiptStatuses.Posted, StringComparison.OrdinalIgnoreCase);
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

    private async Task<IReadOnlyList<ComplianceCoreFactPublicationItem>> BuildVendorRestrictionFactsAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        var entity = await db.VendorRestrictions.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (entity is null)
        {
            return Array.Empty<ComplianceCoreFactPublicationItem>();
        }

        var scopeKey = ScopeForVendor(entity.ExternalPartyId);
        var blocks = string.Equals(entity.Status, VendorRestrictionStatuses.Active, StringComparison.OrdinalIgnoreCase);
        return
        [
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.VendorRestrictionBlocksProcurement,
                scopeKey,
                blocks,
                "vendor_restriction",
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

        var activeRestriction = await db.VendorRestrictions.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == outboxEvent.TenantId
                    && x.ExternalPartyId == entity.ExternalPartyId
                    && x.Status == VendorRestrictionStatuses.Active,
                cancellationToken);

        var scopeKey = ScopeForVendor(entity.ExternalPartyId);
        return
        [
            BooleanFact(
                outboxEvent,
                SupplyArrComplianceCoreFactKeys.VendorRestrictionBlocksProcurement,
                scopeKey,
                activeRestriction,
                "supplier_incident",
                entity.Id)
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

    private static string ScopeForReceivingReceipt(Guid id) => $"receiving_receipt:{id:D}".ToLowerInvariant();

    private static string ScopeForVendor(Guid partyId) => $"vendor:{partyId:D}".ToLowerInvariant();

    private static string ScopeForProcurementException(Guid id) => $"procurement_exception:{id:D}".ToLowerInvariant();
}
