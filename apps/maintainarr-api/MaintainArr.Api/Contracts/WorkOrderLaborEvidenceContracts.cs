namespace MaintainArr.Api.Contracts;

public sealed record WorkOrderTaskLineResponse(
    Guid TaskLineId,
    Guid WorkOrderId,
    string Title,
    string Description,
    int SortOrder,
    string Status,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record CreateWorkOrderTaskLineRequest(
    string Title,
    string? Description,
    int? SortOrder);

public sealed record WorkOrderLaborEntryResponse(
    Guid LaborEntryId,
    Guid WorkOrderId,
    Guid? WorkOrderTaskLineId,
    string PersonId,
    decimal HoursWorked,
    string LaborTypeKey,
    string? Notes,
    Guid LoggedByUserId,
    DateTimeOffset LoggedAt);

public sealed record CreateWorkOrderLaborEntryRequest(
    string PersonId,
    decimal HoursWorked,
    string LaborTypeKey,
    Guid? WorkOrderTaskLineId,
    string? Notes);

public sealed record CreateWorkOrderEvidenceRequest(
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Notes);

public sealed record WorkOrderEvidenceResponse(
    Guid EvidenceId,
    Guid WorkOrderId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);
