namespace SupplyArr.Api.Contracts;

public sealed record WarrantyClaimResponse(
    Guid WarrantyClaimId,
    string ClaimKey,
    string Status,
    string ClaimType,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid? PurchaseOrderId,
    string? PurchaseOrderKey,
    Guid? PurchaseOrderLineId,
    Guid? ReceivingReceiptId,
    string? ReceivingReceiptKey,
    Guid? ReceivingReceiptLineId,
    decimal QuantityClaimed,
    string ProblemDescription,
    string VendorRmaNumber,
    string VendorDisposition,
    string VendorResponseNotes,
    string ClosureNotes,
    string DenialReason,
    Guid CreatedByUserId,
    Guid? SubmittedByUserId,
    DateTimeOffset? SubmittedAt,
    Guid? VendorRespondedByUserId,
    DateTimeOffset? VendorRespondedAt,
    Guid? ClosedByUserId,
    DateTimeOffset? ClosedAt,
    Guid? DeniedByUserId,
    DateTimeOffset? DeniedAt,
    string CancellationReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateWarrantyClaimRequest(
    string ClaimKey,
    string ClaimType,
    Guid VendorPartyId,
    Guid PartId,
    decimal QuantityClaimed,
    string ProblemDescription,
    Guid? PurchaseOrderId,
    Guid? PurchaseOrderLineId,
    Guid? ReceivingReceiptId,
    Guid? ReceivingReceiptLineId,
    string? VendorRmaNumber);

public sealed record UpdateWarrantyClaimRequest(
    string ClaimType,
    decimal QuantityClaimed,
    string ProblemDescription,
    Guid? PurchaseOrderId,
    Guid? PurchaseOrderLineId,
    Guid? ReceivingReceiptId,
    Guid? ReceivingReceiptLineId,
    string? VendorRmaNumber);

public sealed record SubmitWarrantyClaimRequest(string? Notes);

public sealed record RecordWarrantyClaimVendorResponseRequest(
    string VendorDisposition,
    string VendorResponseNotes,
    string? VendorRmaNumber);

public sealed record CloseWarrantyClaimRequest(string ClosureNotes);

public sealed record DenyWarrantyClaimRequest(string DenialReason);

public sealed record CancelWarrantyClaimRequest(string Reason);
