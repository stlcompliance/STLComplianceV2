namespace SupplyArr.Api.Contracts;

public sealed record WarrantyClaimResponse(
    Guid WarrantyClaimId,
    string ClaimKey,
    string Status,
    string ClaimType,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
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
    string SupplierRmaNumber,
    string SupplierDisposition,
    string SupplierResponseNotes,
    string ClosureNotes,
    string DenialReason,
    Guid CreatedByUserId,
    Guid? SubmittedByUserId,
    DateTimeOffset? SubmittedAt,
    Guid? SupplierRespondedByUserId,
    DateTimeOffset? SupplierRespondedAt,
    Guid? ClosedByUserId,
    DateTimeOffset? ClosedAt,
    Guid? DeniedByUserId,
    DateTimeOffset? DeniedAt,
    string CancellationReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateSupplierWarrantyClaimRequest(
    string ClaimKey,
    string ClaimType,
    Guid? SupplierUnitId,
    Guid? SupplierId,
    Guid PartId,
    decimal QuantityClaimed,
    string ProblemDescription,
    Guid? PurchaseOrderId,
    Guid? PurchaseOrderLineId,
    Guid? ReceivingReceiptId,
    Guid? ReceivingReceiptLineId,
    string? SupplierRmaNumber);


public sealed record UpdateWarrantyClaimRequest(
    string ClaimType,
    decimal QuantityClaimed,
    string ProblemDescription,
    Guid? PurchaseOrderId,
    Guid? PurchaseOrderLineId,
    Guid? ReceivingReceiptId,
    Guid? ReceivingReceiptLineId,
    string? SupplierRmaNumber);

public sealed record SubmitWarrantyClaimRequest(string? Notes);

public sealed record RecordWarrantyClaimSupplierResponseRequest(
    string SupplierDisposition,
    string SupplierResponseNotes,
    string? SupplierRmaNumber);

public sealed record CloseWarrantyClaimRequest(string ClosureNotes);

public sealed record DenyWarrantyClaimRequest(string DenialReason);

public sealed record CancelWarrantyClaimRequest(string Reason);
