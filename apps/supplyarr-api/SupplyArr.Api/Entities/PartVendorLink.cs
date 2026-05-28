using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartVendorLink : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartId { get; set; }

    public Guid ExternalPartyId { get; set; }

    public string VendorPartNumber { get; set; } = string.Empty;

    public bool IsPreferred { get; set; }

    public decimal? CatalogUnitPrice { get; set; }

    public string CatalogCurrencyCode { get; set; } = "USD";

    public decimal? CatalogMinimumOrderQuantity { get; set; }

    public int? CatalogLeadTimeDays { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Part Part { get; set; } = null!;

    public ExternalParty ExternalParty { get; set; } = null!;
}
