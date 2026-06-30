namespace SupplyArr.Api.Contracts;

public sealed record CreateQuoteRequest(
    Guid RfqId,
    Guid SupplierId,
    string QuoteKey,
    string CurrencyCode,
    string Notes,
    Guid? VendorPartyId = null);

public sealed record UpsertQuoteLineRequest(
    Guid RfqId,
    Guid RfqLineId,
    decimal UnitPrice,
    decimal QuantityQuoted,
    int? LeadTimeDays,
    string Notes);
