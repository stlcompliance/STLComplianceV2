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
    Guid? InvolvedStaffarrPersonId,
    Guid? StaffarrPersonnelIncidentId,
    DateTimeOffset? StaffarrIncidentRoutedAt,
    string StaffarrIncidentRouteStatus,
    Guid? TrainarrIncidentRemediationId,
    DateTimeOffset? TrainarrIncidentRoutedAt,
    string TrainarrIncidentRouteStatus,
    string ResolutionNotes,
    Guid? ResolvedByUserId,
    DateTimeOffset? ResolvedAt,
    Guid? ClosedByUserId,
    DateTimeOffset? ClosedAt,
    string CancellationReason,
    Guid? CancelledByUserId,
    DateTimeOffset? CancelledAt,
    Guid? ReopenedByUserId,
    DateTimeOffset? ReopenedAt,
    string LastReopenReason,
    int ReopenCount,
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
    Guid? AssignedToUserId,
    Guid? InvolvedStaffarrPersonId = null);

public sealed record UpdateSupplierIncidentRequest(
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    Guid? AssignedToUserId,
    Guid? InvolvedStaffarrPersonId = null);

public sealed record ResolveSupplierIncidentRequest(string ResolutionNotes);

public sealed record CloseSupplierIncidentRequest(string? ResolutionNotes);

public sealed record CancelSupplierIncidentRequest(string Reason);

public sealed record ReopenSupplierIncidentRequest(string Reason);

public sealed record ApplySupplierIncidentProcurementRestrictionRequest(
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string? Reason);
