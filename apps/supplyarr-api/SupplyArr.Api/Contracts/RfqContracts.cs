namespace SupplyArr.Api.Contracts;

public sealed record RfqLineResponse(
    Guid LineId,
    int LineNumber,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RfqVendorInvitationResponse(
    Guid InvitationId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string Status,
    DateTimeOffset InvitedAt,
    DateTimeOffset PortalAccessCodeIssuedAt,
    DateTimeOffset PortalAccessExpiresAt,
    string PortalAccessCode,
    string PortalUrl,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName);

public sealed record VendorQuoteLineResponse(
    Guid QuoteLineId,
    Guid RfqLineId,
    int RfqLineNumber,
    Guid PartId,
    string PartKey,
    decimal UnitPrice,
    decimal QuantityQuoted,
    decimal LineTotal,
    int? LeadTimeDays,
    string Notes);

public sealed record VendorQuoteResponse(
    Guid VendorQuoteId,
    Guid RfqId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string QuoteKey,
    string Status,
    string CurrencyCode,
    decimal? TotalAmount,
    int? LeadTimeDays,
    string Notes,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<VendorQuoteLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName)
{
    public Guid SupplierQuoteId => VendorQuoteId;
}

public sealed record RfqResponse(
    Guid RfqId,
    string RfqKey,
    string Title,
    string Notes,
    string Status,
    Guid RequestedByUserId,
    DateTimeOffset? SubmittedAt,
    Guid? AwardedSupplierId,
    string? AwardedSupplierKey,
    string? AwardedSupplierDisplayName,
    Guid? AwardedParentSupplierId,
    string? AwardedParentSupplierDisplayName,
    string? AwardedSupplierUnitKind,
    IReadOnlyList<string> AwardedSupplierServiceTypes,
    Guid? SelectedVendorQuoteId,
    Guid? PurchaseRequestId,
    DateTimeOffset? AwardedAt,
    IReadOnlyList<RfqLineResponse> Lines,
    IReadOnlyList<RfqVendorInvitationResponse> Invitations,
    IReadOnlyList<VendorQuoteResponse> Quotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? AwardedVendorPartyId,
    string? AwardedVendorPartyKey,
    string? AwardedVendorDisplayName)
{
    public Guid? SelectedSupplierQuoteId => SelectedVendorQuoteId;
}

public sealed record CreateRfqRequest(
    string RfqKey,
    string Title,
    string Notes,
    IReadOnlyList<CreateRfqLineRequest>? Lines);

public sealed record CreateRfqLineRequest(
    Guid PartId,
    decimal QuantityRequested,
    string Notes);

public sealed record UpdateRfqRequest(
    string Title,
    string Notes);

public sealed record AddRfqLineRequest(
    Guid PartId,
    decimal QuantityRequested,
    string Notes);

public sealed record UpdateRfqLineRequest(
    decimal QuantityRequested,
    string Notes);

public record InviteRfqSuppliersRequest(
    IReadOnlyList<Guid>? SupplierIds = null,
    IReadOnlyList<Guid>? VendorPartyIds = null);

public sealed record InviteRfqVendorsRequest(
    IReadOnlyList<Guid>? SupplierIds = null,
    IReadOnlyList<Guid>? VendorPartyIds = null)
    : InviteRfqSuppliersRequest(SupplierIds, VendorPartyIds);

public record CreateSupplierQuoteRequest(
    Guid? SupplierId,
    string QuoteKey,
    string CurrencyCode,
    string Notes,
    Guid? VendorPartyId = null);

public sealed record CreateVendorQuoteRequest(
    Guid? SupplierId,
    Guid? VendorPartyId,
    string QuoteKey,
    string CurrencyCode,
    string Notes)
    : CreateSupplierQuoteRequest(
        SupplierId,
        QuoteKey,
        CurrencyCode,
        Notes,
        VendorPartyId)
{
    public CreateVendorQuoteRequest(
        Guid vendorPartyId,
        string quoteKey,
        string currencyCode,
        string notes)
        : this(vendorPartyId, vendorPartyId, quoteKey, currencyCode, notes)
    {
    }
}

public sealed record VendorPortalRfqLineResponse(
    Guid RfqLineId,
    int LineNumber,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes,
    Guid? QuoteLineId,
    decimal? UnitPrice,
    decimal? QuantityQuoted,
    int? LeadTimeDays,
    string QuoteNotes);

public sealed record VendorPortalRfqResponse(
    Guid RfqId,
    string RfqKey,
    string Title,
    string Notes,
    string Status,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    Guid InvitationId,
    string InvitationStatus,
    DateTimeOffset InvitedAt,
    DateTimeOffset PortalAccessExpiresAt,
    Guid? VendorQuoteId,
    string? QuoteKey,
    string? QuoteStatus,
    string? CurrencyCode,
    decimal? TotalAmount,
    int? LeadTimeDays,
    string? QuoteNotes,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<VendorPortalRfqLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName)
{
    public Guid? SupplierQuoteId => VendorQuoteId;
}

public sealed record VendorPortalCreateQuoteRequest(
    string QuoteKey,
    string CurrencyCode,
    string Notes);

public sealed record UpdateVendorQuoteRequest(
    string CurrencyCode,
    string Notes);

public sealed record UpsertVendorQuoteLineRequest(
    Guid RfqLineId,
    decimal UnitPrice,
    decimal QuantityQuoted,
    int? LeadTimeDays,
    string Notes);

public sealed record RfqQuoteLineMetric(
    Guid VendorQuoteId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string QuoteStatus,
    decimal? UnitPrice,
    decimal? LineTotal,
    int? LeadTimeDays,
    bool IsLowestPrice,
    bool IsFastestLeadTime,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName)
{
    public Guid SupplierQuoteId => VendorQuoteId;
}

public sealed record RfqLineComparisonRow(
    Guid RfqLineId,
    int LineNumber,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityRequested,
    IReadOnlyList<RfqQuoteLineMetric> Quotes);

public sealed record RfqQuoteSummary(
    Guid VendorQuoteId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string Status,
    decimal? TotalAmount,
    int? MaxLeadTimeDays,
    int LinesQuoted,
    bool IsSelected,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName)
{
    public Guid SupplierQuoteId => VendorQuoteId;
}

public sealed record RfqQuoteComparisonResponse(
    Guid RfqId,
    string RfqKey,
    string Status,
    IReadOnlyList<RfqLineComparisonRow> Lines,
    IReadOnlyList<RfqQuoteSummary> QuoteSummaries);

public record SelectSupplierQuoteRequest(Guid SupplierQuoteId);

public sealed record SelectVendorQuoteRequest(Guid VendorQuoteId)
    : SelectSupplierQuoteRequest(VendorQuoteId);

public sealed record CreatePurchaseRequestFromRfqRequest(
    string RequestKey,
    string? Title,
    string? Notes);

public sealed record CreatePurchaseRequestFromRfqResponse(
    Guid RfqId,
    Guid PurchaseRequestId,
    PurchaseRequestResponse PurchaseRequest);
