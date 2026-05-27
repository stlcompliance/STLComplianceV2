namespace StaffArr.Api.Contracts;

public sealed record IngestCertificationGrantRequest(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrPublicationId,
    Guid TrainarrAssignmentId,
    string QualificationKey,
    string QualificationName,
    string TrainingDefinitionName,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    string? Notes);

public sealed record CertificationGrantIngestionResponse(
    Guid PersonCertificationId,
    Guid CertificationDefinitionId,
    Guid TrainarrPublicationId,
    string SourceType,
    string Status);
