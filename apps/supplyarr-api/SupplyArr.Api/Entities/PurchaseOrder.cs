using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PurchaseOrder : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string OrderKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = PurchaseOrderStatuses.Draft;

    public Guid PurchaseRequestId { get; set; }

    public Guid VendorPartyId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTimeOffset? IssuedAt { get; set; }

    public Guid? IssuedByUserId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    public string CancellationReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseRequest PurchaseRequest { get; set; } = null!;

    public ExternalParty VendorParty { get; set; } = null!;

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
