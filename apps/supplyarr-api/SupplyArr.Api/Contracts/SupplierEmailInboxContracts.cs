namespace SupplyArr.Api.Contracts;

public sealed record SupplierEmailInboxMessageResponse(
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
    string? LinkedReferenceType,
    Guid? LinkedReferenceId,
    string? LinkedReferenceKey,
    DateTimeOffset ReceivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record SupplierEmailInboxListResponse(
    IReadOnlyList<SupplierEmailInboxMessageResponse> Items);

public sealed record IngestSupplierEmailInboxRequest(
    string MessageKey,
    string MessageKind,
    string SenderEmail,
    string SenderName,
    string Subject,
    string Body,
    string? ReferenceKey);

public sealed record IngestSupplierEmailInboxResponse(
    bool WasDuplicate,
    SupplierEmailInboxMessageResponse Message);
