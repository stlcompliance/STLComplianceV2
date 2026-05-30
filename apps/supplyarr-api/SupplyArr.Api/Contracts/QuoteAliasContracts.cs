namespace SupplyArr.Api.Contracts;

public sealed record CreateQuoteRequest(
    Guid RfqId,
    Guid VendorPartyId,
    string QuoteKey,
    string CurrencyCode,
    string Notes);

public sealed record UpsertQuoteLineRequest(
    Guid RfqId,
    Guid RfqLineId,
    decimal UnitPrice,
    decimal QuantityQuoted,
    int? LeadTimeDays,
    string Notes);

