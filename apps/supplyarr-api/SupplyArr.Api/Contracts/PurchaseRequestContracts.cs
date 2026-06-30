using System.Text.Json.Serialization;

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
    Guid? SupplierId,
    string? SupplierKey,
    string? SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string? SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
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
    bool IsEmergency,
    string EmergencyReason,
    DateTimeOffset? EmergencyExpeditedAt,
    bool ManagerOverrideApproved,
    string ManagerOverrideJustification,
    DateTimeOffset? ManagerOverrideApprovedAt,
    IReadOnlyList<PurchaseRequestLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePurchaseRequestLineRequest(
    Guid PartId,
    decimal QuantityRequested,
    string Notes);

[method: JsonConstructor]
public sealed record CreatePurchaseRequestRequest(
    string RequestKey,
    string Title,
    string Notes,
    Guid? SupplierId = null,
    Guid? SupplierUnitId = null,
    Guid? VendorPartyId = null,
    IReadOnlyList<CreatePurchaseRequestLineRequest>? Lines = null)
{
    public CreatePurchaseRequestRequest(
        string requestKey,
        string title,
        string notes,
        Guid? vendorPartyId,
        IReadOnlyList<CreatePurchaseRequestLineRequest>? lines)
        : this(requestKey, title, notes, vendorPartyId, vendorPartyId, vendorPartyId, lines)
    {
    }
}

public sealed record UpdatePurchaseRequestRequest(
    string Title,
    string Notes,
    Guid? SupplierId = null,
    Guid? SupplierUnitId = null,
    Guid? VendorPartyId = null);

public sealed record AddPurchaseRequestLineRequest(
    Guid PartId,
    decimal QuantityRequested,
    string Notes);

public sealed record UpdatePurchaseRequestLineRequest(
    decimal QuantityRequested,
    string Notes);

public sealed record RejectPurchaseRequestRequest(string Reason);
