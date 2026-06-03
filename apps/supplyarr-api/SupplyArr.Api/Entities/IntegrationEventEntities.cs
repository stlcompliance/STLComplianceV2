using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantIntegrationEventSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int MaxAttempts { get; set; } = 5;

    public int RetryIntervalMinutes { get; set; } = 15;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class IntegrationOutboxEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = IntegrationEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public sealed class IntegrationInboxEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SourceProduct { get; set; } = string.Empty;

    public string EventKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public string? RelatedEntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = IntegrationEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public sealed class IntegrationEventProcessingRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public int OutboxProcessedCount { get; set; }

    public int InboxProcessedCount { get; set; }

    public int SkippedCount { get; set; }

    public int AbandonedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class IntegrationEventStatuses
{
    public const string Pending = "pending";

    public const string Processed = "processed";

    public const string Abandoned = "abandoned";
}

public static class IntegrationOutboxEventKinds
{
    public const string PartyCreated = "party.created";

    public const string PartCreated = "part.created";

    public const string SupplyArrVendorCreated = "supplyarr.vendor.created";

    public const string SupplyArrVendorUpdated = "supplyarr.vendor.updated";

    public const string SupplyArrVendorApproved = "supplyarr.vendor.approved";

    public const string SupplyArrVendorBlocked = "supplyarr.vendor.blocked";

    public const string SupplyArrCustomerCreated = "supplyarr.customer.created";

    public const string SupplyArrItemCreated = "supplyarr.item.created";

    public const string SupplyArrItemUpdated = "supplyarr.item.updated";

    public const string SupplyArrInventoryReserved = "supplyarr.inventory.reserved";

    public const string PurchaseRequestSubmitted = "purchase_request.submitted";

    public const string PurchaseRequestApproved = "purchase_request.approved";

    public const string PurchaseOrderIssued = "purchase_order.issued";

    public const string ReceivingReceiptPosted = "receiving_receipt.posted";

    public const string ReceivingExceptionCreated = "receiving_exception.created";

    public const string ReceivingExceptionResolved = "receiving_exception.resolved";

    public const string ReceivingExceptionCancelled = "receiving_exception.cancelled";

    public const string ReceivingExceptionReopened = "receiving_exception.reopened";

    public const string MaintainarrDemandReceived = "maintainarr.demand.received";

    public const string RoutarrDemandReceived = "routarr.demand.received";

    public const string TrainarrDemandReceived = "trainarr.demand.received";

    public const string StaffarrDemandReceived = "staffarr.demand.received";

    public const string RfqSubmitted = "rfq.submitted";

    public const string RfqVendorsInvited = "rfq.vendors.invited";

    public const string RfqQuoteSubmitted = "rfq.quote.submitted";

    public const string RfqAwarded = "rfq.awarded";

    public const string SupplierOnboardingSubmitted = "supplier_onboarding.submitted";

    public const string SupplierOnboardingApproved = "supplier_onboarding.approved";

    public const string SupplierOnboardingRejected = "supplier_onboarding.rejected";

    public const string SupplierOnboardingSuspended = "supplier_onboarding.suspended";

    public const string PartyComplianceDocumentRegistered = "party_compliance_document.registered";

    public const string PartyComplianceDocumentApproved = "party_compliance_document.approved";

    public const string PartyComplianceDocumentRejected = "party_compliance_document.rejected";

    public const string VendorRestrictionCreated = "vendor_restriction.created";

    public const string VendorRestrictionUpdated = "vendor_restriction.updated";

    public const string VendorRestrictionLifted = "vendor_restriction.lifted";

    public const string SupplierIncidentCreated = "supplier_incident.created";

    public const string SupplierIncidentUpdated = "supplier_incident.updated";

    public const string SupplierIncidentInvestigating = "supplier_incident.investigating";

    public const string SupplierIncidentResolved = "supplier_incident.resolved";

    public const string SupplierIncidentClosed = "supplier_incident.closed";

    public const string SupplierIncidentCancelled = "supplier_incident.cancelled";

    public const string SupplierIncidentReopened = "supplier_incident.reopened";

    public const string SupplierIncidentRestrictionApplied = "supplier_incident.restriction_applied";

    public const string ProcurementExceptionCreated = "procurement_exception.created";

    public const string ProcurementExceptionUpdated = "procurement_exception.updated";

    public const string ProcurementExceptionInvestigating = "procurement_exception.investigating";

    public const string ProcurementExceptionResolved = "procurement_exception.resolved";

    public const string ProcurementExceptionWaiveRequested = "procurement_exception.waive_requested";

    public const string ProcurementExceptionWaived = "procurement_exception.waived";

    public const string ProcurementExceptionWaiveRejected = "procurement_exception.waive_rejected";

    public const string ProcurementExceptionClosed = "procurement_exception.closed";

    public const string ProcurementExceptionCancelled = "procurement_exception.cancelled";

    public const string ProcurementExceptionReopened = "procurement_exception.reopened";

    public const string WarrantyClaimCreated = "warranty_claim.created";

    public const string WarrantyClaimUpdated = "warranty_claim.updated";

    public const string WarrantyClaimSubmitted = "warranty_claim.submitted";

    public const string WarrantyClaimVendorResponded = "warranty_claim.vendor_responded";

    public const string WarrantyClaimClosed = "warranty_claim.closed";

    public const string WarrantyClaimDenied = "warranty_claim.denied";

    public const string WarrantyClaimCancelled = "warranty_claim.cancelled";

    public const string EmergencyPurchaseCreated = "emergency_purchase.created";

    public const string EmergencyPurchaseExpeditedSubmitted = "emergency_purchase.expedited_submitted";

    public const string EmergencyPurchaseManagerOverrideApproved = "emergency_purchase.manager_override_approved";

    public const string EmergencyPurchaseOrderIssued = "emergency_purchase.purchase_order_issued";
}

public static class IntegrationInboxEventKinds
{
    public const string MaintainarrDemandIngest = "maintainarr.demand.ingest";

    public const string RoutarrDemandIngest = "routarr.demand.ingest";

    public const string TrainarrDemandIngest = "trainarr.demand.ingest";

    public const string StaffarrDemandIngest = "staffarr.demand.ingest";
}
