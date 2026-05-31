namespace MaintainArr.Api.Contracts;

public sealed record MaintainArrDocumentResponse(
    Guid DocumentId,
    string TargetType,
    Guid TargetId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);

public sealed record CreateMaintainArrDocumentRequest(
    string TargetType,
    Guid TargetId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Notes);

public sealed record MaintainArrDocumentAlertResponse(
    string AlertType,
    string TargetType,
    Guid TargetId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string Title,
    string Message,
    DateTimeOffset DetectedAt);
