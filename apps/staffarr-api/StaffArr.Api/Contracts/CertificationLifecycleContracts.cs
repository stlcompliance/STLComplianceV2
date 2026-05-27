namespace StaffArr.Api.Contracts;

public sealed record IngestCertificationLifecycleRequest(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrGrantPublicationId,
    Guid TrainarrLifecyclePublicationId,
    string LifecycleAction,
    string QualificationKey,
    string QualificationName,
    string Message,
    DateTimeOffset? ExpiresAt);

public sealed record CertificationLifecycleIngestionResponse(
    Guid PersonCertificationId,
    Guid TrainarrGrantPublicationId,
    Guid TrainarrLifecyclePublicationId,
    string LifecycleAction,
    string CertificationStatus,
    Guid? TrainingBlockerId);
