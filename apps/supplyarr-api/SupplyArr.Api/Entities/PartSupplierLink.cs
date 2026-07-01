using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartSupplierLink : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartId { get; set; }

    public Guid SupplierId { get; set; }

    public string SupplierPartNumber { get; set; } = string.Empty;

    public bool IsPreferred { get; set; }

    public decimal? CatalogUnitPrice { get; set; }

    public string CatalogCurrencyCode { get; set; } = "USD";

    public decimal? CatalogMinimumOrderQuantity { get; set; }

    public int? CatalogLeadTimeDays { get; set; }

    public decimal? CatalogQuantityAvailable { get; set; }

    public string? CatalogAvailabilityStatus { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Part Part { get; set; } = null!;

    public Supplier Supplier { get; set; } = null!;
}

