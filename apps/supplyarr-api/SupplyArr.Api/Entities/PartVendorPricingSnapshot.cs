using STLCompliance.Shared.Data;



namespace SupplyArr.Api.Entities;



public sealed class PartVendorPricingSnapshot : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid PartVendorLinkId { get; set; }



    public string SnapshotKey { get; set; } = string.Empty;



    public decimal UnitPrice { get; set; }



    public string CurrencyCode { get; set; } = "USD";



    public decimal? MinimumOrderQuantity { get; set; }



    public DateTimeOffset EffectiveFrom { get; set; }



    public DateTimeOffset? EffectiveTo { get; set; }



    public string Source { get; set; } = SnapshotSources.Manual;



    public string Notes { get; set; } = string.Empty;



    public Guid CreatedByUserId { get; set; }



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public PartVendorLink PartVendorLink { get; set; } = null!;

}


