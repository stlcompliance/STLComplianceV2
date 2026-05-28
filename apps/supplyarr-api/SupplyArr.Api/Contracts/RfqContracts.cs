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
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    string Status,
    DateTimeOffset InvitedAt);

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
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    string QuoteKey,
    string Status,
    string CurrencyCode,
    decimal? TotalAmount,
    int? LeadTimeDays,
    string Notes,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<VendorQuoteLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RfqResponse(
    Guid RfqId,
    string RfqKey,
    string Title,
    string Notes,
    string Status,
    Guid RequestedByUserId,
    DateTimeOffset? SubmittedAt,
    Guid? AwardedVendorPartyId,
    string? AwardedVendorDisplayName,
    Guid? SelectedVendorQuoteId,
    Guid? PurchaseRequestId,
    DateTimeOffset? AwardedAt,
    IReadOnlyList<RfqLineResponse> Lines,
    IReadOnlyList<RfqVendorInvitationResponse> Invitations,
    IReadOnlyList<VendorQuoteResponse> Quotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

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

public sealed record InviteRfqVendorsRequest(
    IReadOnlyList<Guid> VendorPartyIds);

public sealed record CreateVendorQuoteRequest(
    Guid VendorPartyId,
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
    Guid VendorPartyId,
    string VendorDisplayName,
    string QuoteStatus,
    decimal? UnitPrice,
    decimal? LineTotal,
    int? LeadTimeDays,
    bool IsLowestPrice,
    bool IsFastestLeadTime);

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
    Guid VendorPartyId,
    string VendorDisplayName,
    string Status,
    decimal? TotalAmount,
    int? MaxLeadTimeDays,
    int LinesQuoted,
    bool IsSelected);

public sealed record RfqQuoteComparisonResponse(
    Guid RfqId,
    string RfqKey,
    string Status,
    IReadOnlyList<RfqLineComparisonRow> Lines,
    IReadOnlyList<RfqQuoteSummary> QuoteSummaries);

public sealed record SelectVendorQuoteRequest(Guid VendorQuoteId);

public sealed record CreatePurchaseRequestFromRfqRequest(
    string RequestKey,
    string? Title,
    string? Notes);

public sealed record CreatePurchaseRequestFromRfqResponse(
    Guid RfqId,
    Guid PurchaseRequestId,
    PurchaseRequestResponse PurchaseRequest);
