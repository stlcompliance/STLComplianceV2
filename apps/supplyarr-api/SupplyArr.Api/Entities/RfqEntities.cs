using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class Rfq : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RfqKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = RfqStatuses.Draft;

    public Guid RequestedByUserId { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public Guid? SubmittedByUserId { get; set; }

    public Guid? AwardedVendorPartyId { get; set; }

    public Guid? SelectedVendorQuoteId { get; set; }

    public Guid? PurchaseRequestId { get; set; }

    public DateTimeOffset? AwardedAt { get; set; }

    public Guid? AwardedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty? AwardedVendorParty { get; set; }

    public ICollection<RfqLine> Lines { get; set; } = new List<RfqLine>();

    public ICollection<RfqVendorInvitation> VendorInvitations { get; set; } = new List<RfqVendorInvitation>();

    public ICollection<VendorQuote> VendorQuotes { get; set; } = new List<VendorQuote>();
}

public sealed class RfqLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RfqId { get; set; }

    public int LineNumber { get; set; }

    public Guid PartId { get; set; }

    public decimal QuantityRequested { get; set; }

    public string UnitOfMeasure { get; set; } = "each";

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Rfq Rfq { get; set; } = null!;

    public Part Part { get; set; } = null!;
}

public sealed class RfqVendorInvitation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RfqId { get; set; }

    public Guid VendorPartyId { get; set; }

    public string Status { get; set; } = RfqInvitationStatuses.Invited;

    public DateTimeOffset InvitedAt { get; set; }

    public Guid InvitedByUserId { get; set; }

    public Rfq Rfq { get; set; } = null!;

    public ExternalParty VendorParty { get; set; } = null!;
}

public sealed class VendorQuote : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RfqId { get; set; }

    public Guid VendorPartyId { get; set; }

    public string QuoteKey { get; set; } = string.Empty;

    public string Status { get; set; } = VendorQuoteStatuses.Draft;

    public string CurrencyCode { get; set; } = "USD";

    public decimal? TotalAmount { get; set; }

    public int? LeadTimeDays { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset? SubmittedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Rfq Rfq { get; set; } = null!;

    public ExternalParty VendorParty { get; set; } = null!;

    public ICollection<VendorQuoteLine> Lines { get; set; } = new List<VendorQuoteLine>();
}

public sealed class VendorQuoteLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid VendorQuoteId { get; set; }

    public Guid RfqLineId { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal QuantityQuoted { get; set; }

    public int? LeadTimeDays { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public VendorQuote VendorQuote { get; set; } = null!;

    public RfqLine RfqLine { get; set; } = null!;
}

public static class RfqStatuses
{
    public const string Draft = "draft";

    public const string Submitted = "submitted";

    public const string Awarded = "awarded";

    public const string Closed = "closed";

    public const string Cancelled = "cancelled";

    public static readonly HashSet<string> Editable = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
    };

    public static readonly HashSet<string> OpenForQuotes = new(StringComparer.OrdinalIgnoreCase)
    {
        Submitted,
    };
}

public static class RfqInvitationStatuses
{
    public const string Invited = "invited";

    public const string Responded = "responded";
}

public static class VendorQuoteStatuses
{
    public const string Draft = "draft";

    public const string Submitted = "submitted";

    public const string Selected = "selected";

    public const string Rejected = "rejected";

    public const string Withdrawn = "withdrawn";

    public static readonly HashSet<string> Editable = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
    };
}
