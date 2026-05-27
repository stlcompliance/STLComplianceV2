using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PurchaseRequestLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PurchaseRequestId { get; set; }

    public int LineNumber { get; set; }

    public Guid PartId { get; set; }

    public decimal QuantityRequested { get; set; }

    public string UnitOfMeasure { get; set; } = "each";

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseRequest PurchaseRequest { get; set; } = null!;

    public Part Part { get; set; } = null!;
}
