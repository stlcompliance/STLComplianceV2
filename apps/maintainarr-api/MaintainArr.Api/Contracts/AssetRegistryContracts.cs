namespace MaintainArr.Api.Contracts;

public sealed record AssetClassResponse(
    Guid AssetClassId,
    string ClassKey,
    string Name,
    string Description,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record CreateAssetClassRequest(
    string ClassKey,
    string Name,
    string Description);

public sealed record UpdateAssetClassRequest(
    string Name,
    string Description);

public sealed record UpdateAssetClassStatusRequest(string Status);

public sealed record AssetTypeResponse(
    Guid AssetTypeId,
    Guid AssetClassId,
    string ClassKey,
    string ClassName,
    string TypeKey,
    string Name,
    string Description,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record CreateAssetTypeRequest(
    Guid AssetClassId,
    string TypeKey,
    string Name,
    string Description);

public sealed record UpdateAssetTypeRequest(
    string Name,
    string Description);

public sealed record UpdateAssetTypeStatusRequest(string Status);

public sealed record AssetResponse(
    Guid AssetId,
    Guid AssetTypeId,
    string TypeKey,
    string TypeName,
    string ClassKey,
    string ClassName,
    string AssetTag,
    string Name,
    string Description,
    string LifecycleStatus,
    string? SiteRef,
    Guid? StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateAssetRequest(
    Guid AssetTypeId,
    string AssetTag,
    string Name,
    string Description,
    string? SiteRef,
    Guid? StaffarrSiteOrgUnitId = null);

public sealed record UpdateAssetRequest(
    string Name,
    string Description,
    string? SiteRef,
    Guid? StaffarrSiteOrgUnitId = null);

public sealed record UpdateAssetLifecycleStatusRequest(string LifecycleStatus);
