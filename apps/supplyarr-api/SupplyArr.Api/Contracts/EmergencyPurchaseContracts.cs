using System.Text.Json.Serialization;

namespace SupplyArr.Api.Contracts;

public sealed record EmergencyPurchaseResponse(
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
    string EmergencyReason,
    DateTimeOffset? EmergencyExpeditedAt,
    bool ManagerOverrideApproved,
    string ManagerOverrideJustification,
    DateTimeOffset? ManagerOverrideApprovedAt,
    Guid? LinkedPurchaseOrderId,
    string? LinkedPurchaseOrderKey,
    IReadOnlyList<PurchaseRequestLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

[method: JsonConstructor]
public sealed record CreateEmergencyPurchaseRequest(
    string RequestKey,
    string Title,
    string EmergencyReason,
    Guid? SupplierId,
    string Notes,
    IReadOnlyList<CreatePurchaseRequestLineRequest> Lines);

public sealed record ExpeditedSubmitEmergencyPurchaseRequest(string? Notes);

public sealed record ManagerOverrideApproveEmergencyPurchaseRequest(string Justification);

public sealed record IssueEmergencyPurchaseOrderRequest(
    string OrderKey,
    string? Title,
    string? Notes);

public sealed record IssueEmergencyPurchaseOrderResponse(
    Guid PurchaseRequestId,
    Guid PurchaseOrderId,
    EmergencyPurchaseResponse EmergencyPurchase,
    PurchaseOrderResponse PurchaseOrder);
