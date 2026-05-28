using STLCompliance.Shared.Data;



namespace SupplyArr.Api.Entities;



public sealed class VendorReturn : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public string ReturnKey { get; set; } = string.Empty;



    public string Status { get; set; } = VendorReturnStatuses.Draft;



    public string SourceType { get; set; } = string.Empty;



    public Guid VendorPartyId { get; set; }



    public Guid? PurchaseOrderId { get; set; }



    public Guid InventoryBinId { get; set; }



    public string RmaNumber { get; set; } = string.Empty;



    public string Notes { get; set; } = string.Empty;



    public Guid CreatedByUserId { get; set; }



    public Guid? PostedByUserId { get; set; }



    public DateTimeOffset? PostedAt { get; set; }



    public Guid? CancelledByUserId { get; set; }



    public DateTimeOffset? CancelledAt { get; set; }



    public string CancellationReason { get; set; } = string.Empty;



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public ExternalParty VendorParty { get; set; } = null!;



    public PurchaseOrder? PurchaseOrder { get; set; }



    public InventoryBin InventoryBin { get; set; } = null!;



    public List<VendorReturnLine> Lines { get; set; } = [];

}

