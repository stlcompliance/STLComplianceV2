namespace RoutArr.Api.Contracts;

public sealed record DriverResponse(
    string PersonId,
    string DisplayName,
    DateTimeOffset MirroredAt);

public sealed record DriverListResponse(IReadOnlyList<DriverResponse> Items);

public sealed record UpsertDriverRequest(
    string PersonId,
    string DisplayName,
    DateTimeOffset? SourceUpdatedAt);

public sealed record VehicleRefResponse(
    string VehicleRefKey,
    string DisplayLabel,
    string? AssetTag,
    DateTimeOffset? MirroredAt,
    bool FromMirror);

public sealed record VehicleRefListResponse(IReadOnlyList<VehicleRefResponse> Items);

public sealed record UpsertVehicleRefRequest(
    string VehicleRefKey,
    string DisplayLabel,
    string? AssetTag,
    DateTimeOffset? SourceUpdatedAt);
