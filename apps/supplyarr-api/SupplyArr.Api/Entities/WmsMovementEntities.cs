using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class WmsStockLedgerEntry : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid MovementGroupId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public Guid PartId { get; set; }
    public Guid InventoryBinId { get; set; }
    public Guid? RelatedInventoryBinId { get; set; }
    public decimal QuantityOnHandDelta { get; set; }
    public decimal QuantityReservedDelta { get; set; }
    public decimal QuantityOnHandAfter { get; set; }
    public decimal QuantityReservedAfter { get; set; }
    public string SourceType { get; set; } = "manual";
    public Guid? SourceReferenceId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Part? Part { get; set; }
    public InventoryBin? InventoryBin { get; set; }
    public InventoryBin? RelatedInventoryBin { get; set; }
}

public sealed class WmsOutboundShipment : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ShipmentKey { get; set; } = string.Empty;
    public string Status { get; set; } = WmsOutboundShipmentStatuses.Created;
    public string ShipVia { get; set; } = WmsShipVia.Manual;
    public string DestinationName { get; set; } = string.Empty;
    public string DestinationAddressSnapshot { get; set; } = string.Empty;
    public Guid? RoutarrShipmentIntentId { get; set; }
    public Guid? RoutarrRouteId { get; set; }
    public string RoutarrStatus { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<WmsOutboundShipmentLine> Lines { get; set; } = [];
}

public sealed class WmsOutboundShipmentLine : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid OutboundShipmentId { get; set; }
    public Guid PartId { get; set; }
    public Guid FromInventoryBinId { get; set; }
    public decimal QuantityRequested { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityPicked { get; set; }
    public decimal QuantityShipped { get; set; }
    public string Status { get; set; } = WmsOutboundShipmentStatuses.Created;

    public WmsOutboundShipment? OutboundShipment { get; set; }
    public Part? Part { get; set; }
    public InventoryBin? FromInventoryBin { get; set; }
}

public static class WmsMovementTypes
{
    public const string TransferOut = "transfer_out";
    public const string TransferIn = "transfer_in";
    public const string Reserve = "reserve";
    public const string Pick = "pick";
    public const string Ship = "ship";
    public const string Cancel = "cancel";
}

public static class WmsShipVia
{
    public const string Manual = "manual";
    public const string RoutArr = "routarr";
}

public static class WmsOutboundShipmentStatuses
{
    public const string Created = "created";
    public const string Reserved = "reserved";
    public const string Picked = "picked";
    public const string Shipped = "shipped";
    public const string Cancelled = "cancelled";
}
