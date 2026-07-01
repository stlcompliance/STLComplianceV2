using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class SupplierOrder : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? BrokerOrderId { get; set; }

    public string? BrokerOrderNumberSnapshot { get; set; }

    public Guid SupplierId { get; set; }

    public string SupplierNameSnapshot { get; set; } = string.Empty;

    public Guid? SupplierLocationId { get; set; }

    public string? PickupLocationNameSnapshot { get; set; }

    public string PickupAddressSnapshot { get; set; } = string.Empty;

    public string? CustomerIdSnapshot { get; set; }

    public string? DeliveryLocationNameSnapshot { get; set; }

    public string? DeliveryAddressSnapshot { get; set; }

    public string ItemDescription { get; set; } = string.Empty;

    public decimal OrderedQuantity { get; set; }

    public decimal QuantityReady { get; set; }

    public decimal QuantityRemaining { get; set; }

    public string QuantityUom { get; set; } = SupplierOrderDefaults.DefaultQuantityUom;

    public DateTimeOffset? ExpectedReadyAt { get; set; }

    public DateTimeOffset? ConfirmedReadyAt { get; set; }

    public DateTimeOffset? PickupWindowStart { get; set; }

    public DateTimeOffset? PickupWindowEnd { get; set; }

    public string? PickupInstructions { get; set; }

    public string Status { get; set; } = SupplierOrderStatuses.Draft;

    public string? CreatedByPersonId { get; set; }

    public Guid? ParentSupplierOrderId { get; set; }

    public string? SplitReason { get; set; }

    public Guid? SplitFromStatusUpdateId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public Supplier? Supplier { get; set; }

    public SupplierOrder? ParentSupplierOrder { get; set; }

    public ICollection<SupplierOrder> ChildSupplierOrders { get; set; } = [];

    public ICollection<SupplierOrderStatusUpdate> StatusUpdates { get; set; } = [];

    public ICollection<SupplierOrderMagicLink> MagicLinks { get; set; } = [];

    public ICollection<SupplierOrderDocumentLink> Documents { get; set; } = [];

    public ICollection<SupplierOrderBrokerDecision> BrokerDecisions { get; set; } = [];
}

public sealed class SupplierOrderStatusUpdate : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SupplierOrderId { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public decimal OrderedQuantitySnapshot { get; set; }

    public decimal QuantityReady { get; set; }

    public decimal QuantityRemaining { get; set; }

    public DateTimeOffset? EstimatedReadyAt { get; set; }

    public DateTimeOffset? ConfirmedReadyAt { get; set; }

    public DateTimeOffset? PickupWindowStart { get; set; }

    public DateTimeOffset? PickupWindowEnd { get; set; }

    public string? Note { get; set; }

    public string? ExceptionReason { get; set; }

    public string Source { get; set; } = SupplierOrderStatusUpdateSources.System;

    public Guid? SubmittedBySupplierContactId { get; set; }

    public string? SubmittedByPersonId { get; set; }

    public Guid? SubmittedByMagicLinkId { get; set; }

    public string? SubmittedIpHash { get; set; }

    public string? SubmittedUserAgentHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public SupplierOrder? SupplierOrder { get; set; }
}

public sealed class SupplierOrderMagicLink : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SupplierOrderId { get; set; }

    public Guid SupplierId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public SupplierOrder? SupplierOrder { get; set; }
}

public sealed class SupplierOrderDocumentLink : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SupplierOrderId { get; set; }

    public string DocumentType { get; set; } = SupplierOrderDocumentTypes.Other;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string? StorageProvider { get; set; }

    public string? StorageKey { get; set; }

    public string RecordArrRecordId { get; set; } = string.Empty;

    public string RecordArrRecordNumberSnapshot { get; set; } = string.Empty;

    public string RecordArrFileId { get; set; } = string.Empty;

    public string? UploadedBySupplierContactId { get; set; }

    public string? UploadedByPersonId { get; set; }

    public Guid? UploadedByMagicLinkId { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public SupplierOrder? SupplierOrder { get; set; }
}

public sealed class SupplierOrderBrokerDecision : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SupplierOrderId { get; set; }

    public string DecisionType { get; set; } = SupplierOrderBrokerDecisionTypes.WaitFull;

    public decimal? AuthorizedQuantity { get; set; }

    public Guid? SelectedTripId { get; set; }

    public string? Note { get; set; }

    public string? DecidedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public SupplierOrder? SupplierOrder { get; set; }
}

public sealed class TenantSupplierOrderSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool AllowDestinationSummaryInSupplierPortal { get; set; }

    public int MagicLinkTtlHours { get; set; } = SupplierOrderDefaults.DefaultMagicLinkTtlHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class SupplierOrderDefaults
{
    public const string DefaultQuantityUom = "each";

    public const int DefaultMagicLinkTtlHours = 72;
}

public static class SupplierOrderStatuses
{
    public const string Draft = "draft";

    public const string SentToSupplier = "sent_to_supplier";

    public const string PendingSupplierAcknowledgment = "pending_supplier_acknowledgment";

    public const string Acknowledged = "acknowledged";

    public const string InProgress = "in_progress";

    public const string PartiallyReady = "partially_ready";

    public const string CompletedReadyForDispatch = "completed_ready_for_dispatch";

    public const string UnableToFulfill = "unable_to_fulfill";

    public const string Cancelled = "cancelled";

    public const string Closed = "closed";

    public const string Split = "split";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        SentToSupplier,
        PendingSupplierAcknowledgment,
        Acknowledged,
        InProgress,
        PartiallyReady,
        CompletedReadyForDispatch,
        UnableToFulfill,
        Cancelled,
        Closed,
        Split,
    };
}

public static class SupplierOrderStatusUpdateSources
{
    public const string BrokerUser = "broker_user";

    public const string SupplierPortal = "supplier_portal";

    public const string MagicLink = "magic_link";

    public const string Api = "api";

    public const string System = "system";
}

public static class SupplierOrderDocumentTypes
{
    public const string Photo = "photo";

    public const string PackingSlip = "packing_slip";

    public const string BillOfLading = "bill_of_lading";

    public const string ScaleTicket = "scale_ticket";

    public const string ProofOfReadiness = "proof_of_readiness";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Photo,
        PackingSlip,
        BillOfLading,
        ScaleTicket,
        ProofOfReadiness,
        Other,
    };
}

public static class SupplierOrderBrokerDecisionTypes
{
    public const string WaitFull = "wait_full";

    public const string DispatchPartial = "dispatch_partial";

    public const string SplitRemaining = "split_remaining";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        WaitFull,
        DispatchPartial,
        SplitRemaining,
    };
}

