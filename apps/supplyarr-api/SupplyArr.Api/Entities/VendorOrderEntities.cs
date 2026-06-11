using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class VendorOrder : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? BrokerOrderId { get; set; }

    public string? BrokerOrderNumberSnapshot { get; set; }

    public Guid VendorId { get; set; }

    public string VendorNameSnapshot { get; set; } = string.Empty;

    public Guid? VendorLocationId { get; set; }

    public string? PickupLocationNameSnapshot { get; set; }

    public string PickupAddressSnapshot { get; set; } = string.Empty;

    public string? CustomerIdSnapshot { get; set; }

    public string? DeliveryLocationNameSnapshot { get; set; }

    public string? DeliveryAddressSnapshot { get; set; }

    public string ItemDescription { get; set; } = string.Empty;

    public decimal OrderedQuantity { get; set; }

    public decimal QuantityReady { get; set; }

    public decimal QuantityRemaining { get; set; }

    public string QuantityUom { get; set; } = VendorOrderDefaults.DefaultQuantityUom;

    public DateTimeOffset? ExpectedReadyAt { get; set; }

    public DateTimeOffset? ConfirmedReadyAt { get; set; }

    public DateTimeOffset? PickupWindowStart { get; set; }

    public DateTimeOffset? PickupWindowEnd { get; set; }

    public string? PickupInstructions { get; set; }

    public string Status { get; set; } = VendorOrderStatuses.Draft;

    public string? CreatedByPersonId { get; set; }

    public Guid? ParentVendorOrderId { get; set; }

    public string? SplitReason { get; set; }

    public Guid? SplitFromStatusUpdateId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public ExternalParty? Vendor { get; set; }

    public VendorOrder? ParentVendorOrder { get; set; }

    public ICollection<VendorOrder> ChildVendorOrders { get; set; } = [];

    public ICollection<VendorOrderStatusUpdate> StatusUpdates { get; set; } = [];

    public ICollection<VendorOrderMagicLink> MagicLinks { get; set; } = [];

    public ICollection<VendorOrderDocumentLink> Documents { get; set; } = [];

    public ICollection<VendorOrderBrokerDecision> BrokerDecisions { get; set; } = [];
}

public sealed class VendorOrderStatusUpdate : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid VendorOrderId { get; set; }

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

    public string Source { get; set; } = VendorOrderStatusUpdateSources.System;

    public Guid? SubmittedByVendorContactId { get; set; }

    public string? SubmittedByPersonId { get; set; }

    public Guid? SubmittedByMagicLinkId { get; set; }

    public string? SubmittedIpHash { get; set; }

    public string? SubmittedUserAgentHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public VendorOrder? VendorOrder { get; set; }
}

public sealed class VendorOrderMagicLink : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid VendorOrderId { get; set; }

    public Guid VendorId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public VendorOrder? VendorOrder { get; set; }
}

public sealed class VendorOrderDocumentLink : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid VendorOrderId { get; set; }

    public string DocumentType { get; set; } = VendorOrderDocumentTypes.Other;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string? StorageProvider { get; set; }

    public string? StorageKey { get; set; }

    public string RecordArrRecordId { get; set; } = string.Empty;

    public string RecordArrRecordNumberSnapshot { get; set; } = string.Empty;

    public string RecordArrFileId { get; set; } = string.Empty;

    public string? UploadedByVendorContactId { get; set; }

    public string? UploadedByPersonId { get; set; }

    public Guid? UploadedByMagicLinkId { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public VendorOrder? VendorOrder { get; set; }
}

public sealed class VendorOrderBrokerDecision : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid VendorOrderId { get; set; }

    public string DecisionType { get; set; } = VendorOrderBrokerDecisionTypes.WaitFull;

    public decimal? AuthorizedQuantity { get; set; }

    public Guid? SelectedTripId { get; set; }

    public string? Note { get; set; }

    public string? DecidedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public VendorOrder? VendorOrder { get; set; }
}

public sealed class TenantVendorOrderSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool AllowDestinationSummaryInVendorPortal { get; set; }

    public int MagicLinkTtlHours { get; set; } = VendorOrderDefaults.DefaultMagicLinkTtlHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class VendorOrderDefaults
{
    public const string DefaultQuantityUom = "each";

    public const int DefaultMagicLinkTtlHours = 72;
}

public static class VendorOrderStatuses
{
    public const string Draft = "draft";

    public const string SentToVendor = "sent_to_vendor";

    public const string PendingVendorAcknowledgment = "pending_vendor_acknowledgment";

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
        SentToVendor,
        PendingVendorAcknowledgment,
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

public static class VendorOrderStatusUpdateSources
{
    public const string BrokerUser = "broker_user";

    public const string VendorPortal = "vendor_portal";

    public const string MagicLink = "magic_link";

    public const string Api = "api";

    public const string System = "system";
}

public static class VendorOrderDocumentTypes
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

public static class VendorOrderBrokerDecisionTypes
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
