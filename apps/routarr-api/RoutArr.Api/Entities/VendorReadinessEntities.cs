using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DispatchBlock : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public string BlockType { get; set; } = DispatchBlockTypes.VendorReadiness;

    public string BlockReason { get; set; } = DispatchBlockReasons.VendorOrderNotComplete;

    public string BlockingEntityType { get; set; } = "vendor_order";

    public string BlockingEntityId { get; set; } = string.Empty;

    public string Status { get; set; } = DispatchBlockStatuses.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public Guid? ResolvedByEventId { get; set; }

    public string? ResolvedByPersonId { get; set; }

    public string? OverrideReason { get; set; }

    public Trip? Trip { get; set; }
}

public sealed class SupplyArrVendorOrderEventReceipt : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid EventId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public Guid VendorOrderId { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }
}

public static class DispatchBlockTypes
{
    public const string VendorReadiness = "vendor_readiness";

    public const string MissingPickupWindow = "missing_pickup_window";

    public const string MissingAssignment = "missing_assignment";

    public const string ComplianceHold = "compliance_hold";

    public const string ManualHold = "manual_hold";
}

public static class DispatchBlockReasons
{
    public const string VendorOrderNotComplete = "vendor_order_not_complete";

    public const string VendorOrderPartiallyReady = "vendor_order_partially_ready";

    public const string VendorUnableToFulfill = "vendor_unable_to_fulfill";

    public const string MissingRequiredFields = "missing_required_fields";

    public const string ManuallyBlocked = "manually_blocked";
}

public static class DispatchBlockStatuses
{
    public const string Active = "active";

    public const string Resolved = "resolved";
}
