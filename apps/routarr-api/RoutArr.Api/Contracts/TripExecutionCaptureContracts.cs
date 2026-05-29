namespace RoutArr.Api.Contracts;

public sealed record TripExecutionSettingsResponse(
    bool RequirePreTripDvirBeforeStart,
    bool RequirePostTripDvirBeforeComplete,
    bool RequireDeliveryProofBeforeComplete,
    bool RequirePickupProofBeforeStart,
    bool BlockTripStartOnDvirFail,
    bool BlockTripCompleteOnDvirFail,
    bool RequirePickupProofPhotoBeforeStart,
    bool RequireDeliveryProofPhotoBeforeComplete,
    bool RequireDeliverySignatureBeforeComplete,
    bool RequirePreTripDvirPhotoBeforeStart,
    bool RequirePostTripDvirPhotoBeforeComplete,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertTripExecutionSettingsRequest(
    bool RequirePreTripDvirBeforeStart,
    bool RequirePostTripDvirBeforeComplete,
    bool RequireDeliveryProofBeforeComplete,
    bool RequirePickupProofBeforeStart,
    bool BlockTripStartOnDvirFail,
    bool BlockTripCompleteOnDvirFail,
    bool RequirePickupProofPhotoBeforeStart,
    bool RequireDeliveryProofPhotoBeforeComplete,
    bool RequireDeliverySignatureBeforeComplete,
    bool RequirePreTripDvirPhotoBeforeStart,
    bool RequirePostTripDvirPhotoBeforeComplete);

public sealed record TripCaptureReadinessItemResponse(
    string Key,
    string Label,
    bool Satisfied,
    bool Required,
    string? Message);

public sealed record TripCaptureReadinessResponse(
    Guid TripId,
    string DispatchStatus,
    bool CanStartTrip,
    bool CanCompleteTrip,
    IReadOnlyList<TripCaptureReadinessItemResponse> Items);
