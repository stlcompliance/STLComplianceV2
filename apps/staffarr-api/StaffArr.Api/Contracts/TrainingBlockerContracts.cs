namespace StaffArr.Api.Contracts;

public sealed record IngestTrainingBlockerRequest(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrPublicationId,
    string QualificationKey,
    string QualificationName,
    string BlockerType,
    string Message,
    DateTimeOffset? ExpiresAt);

public sealed record ClearTrainingBlockerRequest(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrPublicationId);

public sealed record TrainingBlockerIngestionResponse(
    Guid TrainingBlockerId,
    Guid TrainarrPublicationId,
    string Status);
