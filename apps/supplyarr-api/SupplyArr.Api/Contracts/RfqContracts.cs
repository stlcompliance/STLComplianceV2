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

public sealed record RfqSupplierInvitationResponse(
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
    string PortalUrl);

public sealed record SupplierQuoteLineResponse(
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

public sealed record SupplierQuoteResponse(
    Guid SupplierQuoteId,
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
    IReadOnlyList<SupplierQuoteLineResponse> Lines,
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
    Guid? AwardedSupplierId,
    string? AwardedSupplierKey,
    string? AwardedSupplierDisplayName,
    Guid? AwardedParentSupplierId,
    string? AwardedParentSupplierDisplayName,
    string? AwardedSupplierUnitKind,
    IReadOnlyList<string> AwardedSupplierServiceTypes,
    Guid? SelectedSupplierQuoteId,
    Guid? PurchaseRequestId,
    DateTimeOffset? AwardedAt,
    IReadOnlyList<RfqLineResponse> Lines,
    IReadOnlyList<RfqSupplierInvitationResponse> Invitations,
    IReadOnlyList<SupplierQuoteResponse> Quotes,
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

public record InviteRfqSuppliersRequest(
    IReadOnlyList<Guid>? SupplierIds = null);

public record CreateSupplierQuoteRequest(
    Guid? SupplierId,
    string QuoteKey,
    string CurrencyCode,
    string Notes);

public sealed record SupplierPortalRfqLineResponse(
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

public sealed record SupplierPortalRfqResponse(
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
    Guid? SupplierQuoteId,
    string? QuoteKey,
    string? QuoteStatus,
    string? CurrencyCode,
    decimal? TotalAmount,
    int? LeadTimeDays,
    string? QuoteNotes,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<SupplierPortalRfqLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SupplierPortalCreateQuoteRequest(
    string QuoteKey,
    string CurrencyCode,
    string Notes);

public sealed record UpdateSupplierQuoteRequest(
    string CurrencyCode,
    string Notes);

public sealed record UpsertSupplierQuoteLineRequest(
    Guid RfqLineId,
    decimal UnitPrice,
    decimal QuantityQuoted,
    int? LeadTimeDays,
    string Notes);

public sealed record RfqQuoteLineMetric(
    Guid SupplierQuoteId,
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
    Guid SupplierQuoteId,
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
    bool IsSelected);

public sealed record RfqQuoteComparisonResponse(
    Guid RfqId,
    string RfqKey,
    string Status,
    IReadOnlyList<RfqLineComparisonRow> Lines,
    IReadOnlyList<RfqQuoteSummary> QuoteSummaries);

public record SelectSupplierQuoteRequest(Guid SupplierQuoteId);

public sealed record CreatePurchaseRequestFromRfqRequest(
    string RequestKey,
    string? Title,
    string? Notes);

public sealed record CreatePurchaseRequestFromRfqResponse(
    Guid RfqId,
    Guid PurchaseRequestId,
    PurchaseRequestResponse PurchaseRequest);
