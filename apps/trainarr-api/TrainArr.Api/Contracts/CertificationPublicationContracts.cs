namespace TrainArr.Api.Contracts;

public sealed record CreateCertificationPublicationRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string BlockerType,
    string Message,
    DateTimeOffset? ExpiresAt);

public sealed record CertificationPublicationResponse(
    Guid PublicationId,
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string BlockerType,
    string Message,
    string Status,
    DateTimeOffset PublishedAt);
