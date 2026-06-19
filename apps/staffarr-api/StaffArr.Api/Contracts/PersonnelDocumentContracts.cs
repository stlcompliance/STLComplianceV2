namespace StaffArr.Api.Contracts;

public sealed record CreatePersonnelDocumentRequest(
    string DocumentTypeKey,
    string AccessLevel,
    string RetentionCategory,
    bool RestrictedData,
    string Title,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Description,
    DateTimeOffset? ExpiresAt);

public sealed record PersonnelDocumentSummaryResponse(
    Guid DocumentId,
    Guid PersonId,
    string DocumentTypeKey,
    string AccessLevel,
    string RetentionCategory,
    bool RestrictedData,
    string Title,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Description,
    DateTimeOffset? ExpiresAt,
    string Status,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PersonnelDocumentDetailResponse(
    Guid DocumentId,
    Guid PersonId,
    string DocumentTypeKey,
    string AccessLevel,
    string RetentionCategory,
    bool RestrictedData,
    string Title,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Description,
    DateTimeOffset? ExpiresAt,
    string Status,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
