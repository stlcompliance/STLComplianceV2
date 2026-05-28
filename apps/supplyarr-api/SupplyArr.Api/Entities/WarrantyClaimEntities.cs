using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class WarrantyClaim : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ClaimKey { get; set; } = string.Empty;

    public string Status { get; set; } = WarrantyClaimStatuses.Draft;

    public string ClaimType { get; set; } = WarrantyClaimTypes.Defective;

    public Guid VendorPartyId { get; set; }

    public Guid PartId { get; set; }

    public Guid? PurchaseOrderId { get; set; }

    public Guid? PurchaseOrderLineId { get; set; }

    public Guid? ReceivingReceiptId { get; set; }

    public Guid? ReceivingReceiptLineId { get; set; }

    public decimal QuantityClaimed { get; set; }

    public string ProblemDescription { get; set; } = string.Empty;

    public string VendorRmaNumber { get; set; } = string.Empty;

    public string VendorDisposition { get; set; } = string.Empty;

    public string VendorResponseNotes { get; set; } = string.Empty;

    public string ClosureNotes { get; set; } = string.Empty;

    public string DenialReason { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public Guid? SubmittedByUserId { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public Guid? VendorRespondedByUserId { get; set; }

    public DateTimeOffset? VendorRespondedAt { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public Guid? DeniedByUserId { get; set; }

    public DateTimeOffset? DeniedAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string CancellationReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty VendorParty { get; set; } = null!;

    public Part Part { get; set; } = null!;

    public PurchaseOrder? PurchaseOrder { get; set; }

    public PurchaseOrderLine? PurchaseOrderLine { get; set; }

    public ReceivingReceipt? ReceivingReceipt { get; set; }

    public ReceivingReceiptLine? ReceivingReceiptLine { get; set; }
}

public static class WarrantyClaimStatuses
{
    public const string Draft = "draft";

    public const string Submitted = "submitted";

    public const string VendorResponded = "vendor_responded";

    public const string Closed = "closed";

    public const string Denied = "denied";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Submitted,
        VendorResponded,
        Closed,
        Denied,
        Cancelled,
    };

    public static readonly IReadOnlySet<string> Open = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Submitted,
        VendorResponded,
    };
}

public static class WarrantyClaimTypes
{
    public const string Defective = "defective";

    public const string Doa = "doa";

    public const string PrematureFailure = "premature_failure";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Defective,
        Doa,
        PrematureFailure,
        Other,
    };
}

public static class WarrantyClaimVendorDispositions
{
    public const string Approved = "approved";

    public const string PartialCredit = "partial_credit";

    public const string Replacement = "replacement";

    public const string Denied = "denied";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Approved,
        PartialCredit,
        Replacement,
        Denied,
    };
}
