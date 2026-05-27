namespace TrainArr.Api.Contracts;

public sealed record CreateTrainingEvidenceRequest(
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Notes);

public sealed record TrainingEvidenceResponse(
    Guid EvidenceId,
    Guid TrainingAssignmentId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);
