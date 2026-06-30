namespace SupplyArr.Api.Contracts;

public sealed record VendorEmailInboxMessageResponse(
    Guid MessageId,
    string MessageKey,
    string MessageKind,
    string SenderEmail,
    string SenderName,
    string Subject,
    string BodyPreview,
    string MatchStatus,
    string MatchReason,
    Guid? SupplierId,
    string? SupplierKey,
    string? SupplierDisplayName,
    Guid? VendorPartyId,
    string? VendorPartyKey,
    string? VendorDisplayName,
    string? LinkedReferenceType,
    Guid? LinkedReferenceId,
    string? LinkedReferenceKey,
    DateTimeOffset ReceivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record VendorEmailInboxListResponse(
    IReadOnlyList<VendorEmailInboxMessageResponse> Items);

public sealed record IngestVendorEmailInboxRequest(
    string MessageKey,
    string MessageKind,
    string SenderEmail,
    string SenderName,
    string Subject,
    string Body,
    string? ReferenceKey);

public sealed record IngestVendorEmailInboxResponse(
    bool WasDuplicate,
    VendorEmailInboxMessageResponse Message);
