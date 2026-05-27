using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PurchaseRequest : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RequestKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = PurchaseRequestStatuses.Draft;

    public Guid? VendorPartyId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public Guid? SubmittedByUserId { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    public Guid? RejectedByUserId { get; set; }

    public string RejectionReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty? VendorParty { get; set; }

    public ICollection<PurchaseRequestLine> Lines { get; set; } = new List<PurchaseRequestLine>();
}
