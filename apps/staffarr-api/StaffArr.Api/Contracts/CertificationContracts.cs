namespace StaffArr.Api.Contracts;

public sealed record CertificationDefinitionResponse(
    Guid CertificationDefinitionId,
    string CertificationKey,
    string Name,
    string? Description,
    string Category,
    int? DefaultValidityDays,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertCertificationDefinitionRequest(
    string CertificationKey,
    string Name,
    string? Description,
    string Category,
    int? DefaultValidityDays);

public sealed record PersonCertificationResponse(
    Guid PersonCertificationId,
    Guid PersonId,
    Guid CertificationDefinitionId,
    string CertificationKey,
    string CertificationName,
    string Category,
    string SourceType,
    string Status,
    string EffectiveStatus,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    string? Notes,
    Guid? GrantedByUserId,
    Guid? ExternalPublicationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record GrantPersonCertificationRequest(
    Guid CertificationDefinitionId,
    DateTimeOffset? GrantedAt,
    DateTimeOffset? ExpiresAt,
    string? Notes);

public sealed record UpdatePersonCertificationRequest(
    string Status,
    DateTimeOffset? ExpiresAt,
    string? Notes);
