using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartStockReservation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ReservationKey { get; set; } = string.Empty;

    public string Status { get; set; } = StockReservationStatuses.Active;

    public string SourceType { get; set; } = "manual";

    public Guid? SourceReferenceId { get; set; }

    public Guid PartId { get; set; }

    public Guid InventoryBinId { get; set; }

    public Guid PartStockLevelId { get; set; }

    public decimal QuantityReserved { get; set; }

    public string Notes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public Guid? FulfilledByUserId { get; set; }

    public DateTimeOffset? FulfilledAt { get; set; }

    public Guid? ReleasedByUserId { get; set; }

    public DateTimeOffset? ReleasedAt { get; set; }

    public string ReleaseReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Part? Part { get; set; }

    public InventoryBin? InventoryBin { get; set; }

    public PartStockLevel? PartStockLevel { get; set; }
}
