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
    Guid? WaiveRequestedByUserId,
    DateTimeOffset? WaiveRequestedAt,
    Guid? WaivedByUserId,
    DateTimeOffset? WaivedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateProcurementExceptionRequest(
    string ExceptionKey,
    string ExceptionCategory,
    string Title,
    string Description,
    Guid? AssignedToUserId);

public sealed record UpdateProcurementExceptionRequest(
    string Title,
    string Description,
    string ExceptionCategory,
    Guid? AssignedToUserId);

public sealed record ResolveProcurementExceptionRequest(string ResolutionNotes);

public sealed record RequestProcurementExceptionWaiveRequest(string WaiveJustification);

public sealed record RejectProcurementExceptionWaiveRequest(string Reason);

public sealed record CloseProcurementExceptionRequest(string? ResolutionNotes);

public sealed record CancelProcurementExceptionRequest(string Reason);
