namespace MaintainArr.Api.Contracts;

public sealed record CatalogOptionResponse(
    string Key,
    string Label,
    string Description,
    int SortOrder,
    string? ParentOptionKey,
    bool IsActive,
    IReadOnlyDictionary<string, string>? Dependency,
    IReadOnlyDictionary<string, object?>? Metadata);

public sealed record CatalogResponse(
    string Key,
    string Label,
    string Description,
    string Owner,
    string Scope,
    bool IsSystem,
    bool IsTenantExtendable,
    bool IsActive,
    IReadOnlyList<CatalogOptionResponse> Options);

public sealed record UpsertCatalogOptionRequest(
    string Key,
    string Label,
    string Description,
    int SortOrder,
    string? ParentOptionKey,
    bool IsActive,
    IReadOnlyDictionary<string, string>? Dependency,
    IReadOnlyDictionary<string, object?>? Metadata);

public sealed record FieldMetadataResponse(
    string Key,
    string Label,
    string Description,
    string Type,
    string Control,
    bool Required,
    string? CatalogKey,
    string? ReferenceKey,
    string Source,
    string SourceOfTruth,
    string StoredValue,
    string DisplayValue,
    bool AllowCustom,
    bool CustomRequiresApproval,
    bool DrivesLogic,
    bool DrivesInspectionBranching,
    bool DrivesPMApplicability,
    bool DrivesCompliance,
    bool DrivesReporting,
    bool DrivesReadiness,
    IReadOnlyDictionary<string, string>? DependsOn,
    IReadOnlyDictionary<string, object?>? Validation,
    object? DefaultValue,
    IReadOnlyDictionary<string, object?>? Visibility,
    string SectionKey,
    IReadOnlyList<CatalogOptionResponse>? Options);

public sealed record FieldsetResponse(
    string Key,
    string Label,
    string EntityType,
    string Purpose,
    IReadOnlyList<FieldMetadataResponse> Fields);

public sealed record ReferenceOptionResponse(
    string Key,
    string? Id,
    string Label,
    string Source,
    string SourceOfTruth,
    string StoredValue,
    string DisplayValue,
    bool IsActive);

public sealed record AssetUpsertV1Request(
    string AssetTag,
    string Name,
    string? Description,
    IReadOnlyDictionary<string, object?> Values);

public sealed record AssetFieldContextResponse(
    Guid AssetId,
    IReadOnlyList<AssetFieldContextValueResponse> Fields);

public sealed record AssetFieldContextValueResponse(
    string Key,
    object? StoredValue,
    string? DisplayValue,
    string Source,
    string SourceOfTruth);
