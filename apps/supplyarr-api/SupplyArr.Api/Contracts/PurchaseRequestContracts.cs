namespace SupplyArr.Api.Contracts;

public sealed record PurchaseRequestLineResponse(
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

public sealed record PurchaseRequestResponse(
    Guid PurchaseRequestId,
    string RequestKey,
    string Title,
    string Notes,
    string Status,
    Guid? VendorPartyId,
    string? VendorPartyKey,
    string? VendorDisplayName,
    Guid RequestedByUserId,
    DateTimeOffset? SubmittedAt,
    Guid? SubmittedByUserId,
    DateTimeOffset? ApprovedAt,
    Guid? ApprovedByUserId,
    DateTimeOffset? RejectedAt,
    Guid? RejectedByUserId,
    string RejectionReason,
    IReadOnlyList<PurchaseRequestLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePurchaseRequestLineRequest(
    Guid PartId,
    decimal QuantityRequested,
    string Notes);

public sealed record CreatePurchaseRequestRequest(
    string RequestKey,
    string Title,
    string Notes,
    Guid? VendorPartyId,
    IReadOnlyList<CreatePurchaseRequestLineRequest>? Lines);

public sealed record UpdatePurchaseRequestRequest(
    string Title,
    string Notes,
    Guid? VendorPartyId);

public sealed record AddPurchaseRequestLineRequest(
    Guid PartId,
    decimal QuantityRequested,
    string Notes);

public sealed record UpdatePurchaseRequestLineRequest(
    decimal QuantityRequested,
    string Notes);

public sealed record RejectPurchaseRequestRequest(string Reason);
