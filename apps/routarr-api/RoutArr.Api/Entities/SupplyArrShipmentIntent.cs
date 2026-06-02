using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class SupplyArrShipmentIntent : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SupplyarrShipmentId { get; set; }
    public string ShipmentKey { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public string DestinationAddressSnapshot { get; set; } = string.Empty;
    public string Status { get; set; } = "created";
    public Guid? RouteId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<SupplyArrShipmentIntentLine> Lines { get; set; } = [];
}

public sealed class SupplyArrShipmentIntentLine : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ShipmentIntentId { get; set; }
    public Guid SupplyarrShipmentLineId { get; set; }
    public Guid PartId { get; set; }
    public string PartDisplayName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public SupplyArrShipmentIntent? ShipmentIntent { get; set; }
}
