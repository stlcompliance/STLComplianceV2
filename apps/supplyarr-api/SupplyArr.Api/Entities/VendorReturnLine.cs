using STLCompliance.Shared.Data;



namespace SupplyArr.Api.Entities;



public sealed class VendorReturnLine : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid VendorReturnId { get; set; }



    public int LineNumber { get; set; }



    public Guid PartId { get; set; }



    public Guid? PurchaseOrderLineId { get; set; }



    public decimal Quantity { get; set; }



    public string Notes { get; set; } = string.Empty;



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public VendorReturn VendorReturn { get; set; } = null!;



    public Part Part { get; set; } = null!;



    public PurchaseOrderLine? PurchaseOrderLine { get; set; }

}

