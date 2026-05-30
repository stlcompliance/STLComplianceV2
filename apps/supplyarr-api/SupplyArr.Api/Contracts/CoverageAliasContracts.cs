namespace SupplyArr.Api.Contracts;

public sealed record SubstitutionItemResponse(
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid AliasId,
    string AliasKey,
    string ManufacturerName,
    string ManufacturerPartNumber,
    DateTimeOffset CreatedAt);

public sealed record SupplyDocumentItemResponse(
    Guid DocumentId,
    Guid PartyId,
    string PartyKey,
    string PartyDisplayName,
    string PartyType,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    string ReviewStatus,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UpdatedAt);

public sealed record CreateSupplyDocumentRequest(
    Guid PartyId,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageUri);

public sealed record ContractSnapshotItemResponse(
    string ContractType,
    Guid SnapshotId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid PartyId,
    string PartyKey,
    string PartyDisplayName,
    string SnapshotKey,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset UpdatedAt);

public sealed record ImportOptionResponse(
    string ImportType,
    string Description);

public sealed record ExportOptionResponse(
    string ExportType,
    string Description,
    string Endpoint);

public sealed record AdminOverviewResponse(
    string ProductKey,
    string RoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements,
    IReadOnlyList<string> AvailableAdminAreas);

