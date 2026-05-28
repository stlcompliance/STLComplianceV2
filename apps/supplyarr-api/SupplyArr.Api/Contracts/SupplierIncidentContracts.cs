namespace SupplyArr.Api.Contracts;

public sealed record SupplierIncidentResponse(
    Guid IncidentId,
    Guid ExternalPartyId,
    string PartyKey,
    string PartyDisplayName,
    string PartyType,
    string IncidentKey,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    Guid? ReceivingReceiptId,
    Guid? ReceivingExceptionId,
    Guid? VendorRestrictionId,
    Guid ReportedByUserId,
    Guid? AssignedToUserId,
    string ResolutionNotes,
    Guid? ResolvedByUserId,
    DateTimeOffset? ResolvedAt,
    Guid? ClosedByUserId,
    DateTimeOffset? ClosedAt,
    string CancellationReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateSupplierIncidentRequest(
    Guid ExternalPartyId,
    string IncidentKey,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    Guid? ReceivingReceiptId,
    Guid? ReceivingExceptionId,
    Guid? AssignedToUserId);

public sealed record UpdateSupplierIncidentRequest(
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    Guid? AssignedToUserId);

public sealed record ResolveSupplierIncidentRequest(string ResolutionNotes);

public sealed record CloseSupplierIncidentRequest(string? ResolutionNotes);

public sealed record CancelSupplierIncidentRequest(string Reason);

public sealed record ApplySupplierIncidentProcurementRestrictionRequest(
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string? Reason);
