namespace MaintainArr.Api.Contracts;

public sealed record CreateAssetReadinessCheckRequest(
    Guid? AssetId,
    string? VehicleRefKey,
    string? AssetTag,
    string SourceProduct,
    string RequestedBy,
    string? Status);

public sealed record AssetReadinessCheckResponse(
    Guid AssetReadinessCheckId,
    Guid AssetId,
    string? AssetTag,
    string? VehicleRefKey,
    string SourceProduct,
    string RequestedBy,
    string Status,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CreatedAt);
