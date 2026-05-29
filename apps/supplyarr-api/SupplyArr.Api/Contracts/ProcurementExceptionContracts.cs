namespace SupplyArr.Api.Contracts;

public sealed record ProcurementExceptionResponse(
    Guid ExceptionId,
    string ExceptionKey,
    string SubjectType,
    Guid SubjectId,
    string SubjectKey,
    Guid? VendorPartyId,
    string? VendorPartyKey,
    string? VendorDisplayName,
    string ExceptionCategory,
    string Title,
    string Description,
    string Status,
    string ResolutionNotes,
    string WaiveJustification,
    string WaiveRejectionReason,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt,
    bool IsSlaBreached,
    string ResolutionTemplateKey,
    Guid? LinkedPurchaseRequestId,
    string? LinkedPurchaseRequestKey,
    Guid? LinkedPurchaseOrderId,
    string? LinkedPurchaseOrderKey,
    Guid? WaiveRequestedByUserId,
    DateTimeOffset? WaiveRequestedAt,
    Guid? WaivedByUserId,
    DateTimeOffset? WaivedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset? CancelledAt,
    string CancellationReason,
    Guid? ReopenedByUserId,
    DateTimeOffset? ReopenedAt,
    string LastReopenReason,
    int ReopenCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateProcurementExceptionRequest(
    string ExceptionKey,
    string ExceptionCategory,
    string Title,
    string Description,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt = null);

public sealed record UpdateProcurementExceptionRequest(
    string Title,
    string Description,
    string ExceptionCategory,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt = null);

public sealed record ResolveProcurementExceptionRequest(
    string ResolutionNotes,
    string? ResolutionTemplateKey = null);

public sealed record RequestProcurementExceptionWaiveRequest(string WaiveJustification);

public sealed record RejectProcurementExceptionWaiveRequest(string Reason);

public sealed record CloseProcurementExceptionRequest(string? ResolutionNotes);

public sealed record CancelProcurementExceptionRequest(string Reason);

public sealed record ReopenProcurementExceptionRequest(string Reason);
