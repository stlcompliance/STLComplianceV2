namespace RoutArr.Api.Contracts;

public sealed record DispatchBoardStateResponse(
    string DefaultScope,
    DateTimeOffset UpdatedAt,
    Guid? UpdatedByUserId);

public sealed record UpsertDispatchBoardStateRequest(string DefaultScope);

public sealed record StaffarrPersonRefResponse(
    string PersonId,
    string DisplayName,
    DateTimeOffset MirroredAt);

public sealed record StaffarrPersonRefListResponse(
    IReadOnlyList<StaffarrPersonRefResponse> Items);

public sealed record UpsertStaffarrPersonRefRequest(
    string PersonId,
    string DisplayName,
    DateTimeOffset? SourceUpdatedAt);

public sealed record DispatchCommandCenterTripColumn(
    string DispatchStatus,
    string Label,
    int Count,
    IReadOnlyList<TripSummaryResponse> Trips);

public sealed record DispatchCommandCenterActionDescriptor(
    string ActionKey,
    string Label,
    string Route,
    string HttpMethod,
    string Description);

public sealed record DispatchCommandCenterResponse(
    DateTimeOffset GeneratedAt,
    string Scope,
    DispatchBoardStateResponse BoardState,
    DispatchBoardResponse Board,
    IReadOnlyList<DispatchCommandCenterTripColumn> TripColumns,
    StaffarrPersonRefListResponse DriverRefs,
    IReadOnlyList<DispatchCommandCenterActionDescriptor> Actions);
