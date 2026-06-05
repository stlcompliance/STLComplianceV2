namespace MaintainArr.Api.Contracts;

public sealed record CreateAssetQualityHoldRequest(
    Guid AssetId,
    string HoldType,
    string SourceProduct,
    string? SourceObjectRef,
    string Title,
    string Description,
    string Severity,
    string? CreatedByPersonId = null);

public sealed record AssetQualityHoldResponse(
    Guid HoldId,
    Guid AssetId,
    string HoldType,
    string SourceProduct,
    string? SourceObjectRef,
    string Title,
    string Description,
    string Severity,
    string Status,
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId,
    DateTimeOffset? ReleasedAt,
    string? ReleasedByPersonId,
    string? ReleaseReason);

public sealed record ReleaseAssetQualityHoldRequest(
    Guid HoldId,
    string? ReleasedByPersonId,
    string? ReleaseReason);
