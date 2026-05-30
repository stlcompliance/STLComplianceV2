namespace SupplyArr.Api.Contracts;

public sealed record RegisterVendorDocumentRequest(
    Guid PartyId,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageUri);
