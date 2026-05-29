namespace MaintainArr.Api.Contracts;

public sealed record CreateMaintainArrEvidenceRequest(
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Notes,
    Guid? ChecklistItemId = null);

public sealed record DefectEvidenceResponse(
    Guid EvidenceId,
    Guid DefectId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);

public sealed record InspectionRunEvidenceResponse(
    Guid EvidenceId,
    Guid InspectionRunId,
    Guid? ChecklistItemId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);
