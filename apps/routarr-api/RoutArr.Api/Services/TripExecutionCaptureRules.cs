using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public static class TripExecutionCaptureRules
{
    public const string PreTripDvirKey = "pre_trip_dvir";

    public const string PostTripDvirKey = "post_trip_dvir";

    public const string PickupProofKey = "pickup_proof";

    public const string DeliveryProofKey = "delivery_proof";

    public const string PreTripDvirPassKey = "pre_trip_dvir_pass";

    public const string PostTripDvirPassKey = "post_trip_dvir_pass";

    public const string PickupProofPhotoKey = "pickup_proof_photo";

    public const string DeliveryProofPhotoKey = "delivery_proof_photo";

    public const string DeliverySignatureKey = "delivery_signature";

    public const string PreTripDvirPhotoKey = "pre_trip_dvir_photo";

    public const string PostTripDvirPhotoKey = "post_trip_dvir_photo";

    public static void ValidateDvirSubmit(string result, string? defectNotes)
    {
        var normalized = result?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DvirInspectionResults.All.Contains(normalized))
        {
            throw new StlApiException(
                "trip_dvir.invalid_result",
                "DVIR result must be pass, fail, or conditional.",
                400);
        }

        if (normalized is DvirInspectionResults.Fail or DvirInspectionResults.Conditional)
        {
            var notes = defectNotes?.Trim() ?? string.Empty;
            if (notes.Length < 3)
            {
                throw new StlApiException(
                    "trip_dvir.defect_notes_required",
                    "Defect notes are required when DVIR result is fail or conditional.",
                    400);
            }
        }
    }

    public static TripExecutionSettingsSnapshot ResolveSettings(TenantTripExecutionSettings? entity) =>
        entity is null
            ? TripExecutionSettingsSnapshot.Defaults
            : new TripExecutionSettingsSnapshot(
                entity.RequirePreTripDvirBeforeStart,
                entity.RequirePostTripDvirBeforeComplete,
                entity.RequireDeliveryProofBeforeComplete,
                entity.RequirePickupProofBeforeStart,
                entity.BlockTripStartOnDvirFail,
                entity.BlockTripCompleteOnDvirFail,
                entity.RequirePickupProofPhotoBeforeStart,
                entity.RequireDeliveryProofPhotoBeforeComplete,
                entity.RequireDeliverySignatureBeforeComplete,
                entity.RequirePreTripDvirPhotoBeforeStart,
                entity.RequirePostTripDvirPhotoBeforeComplete);

    public static TripCaptureReadinessResponse BuildReadiness(
        Guid tripId,
        string dispatchStatus,
        TripExecutionSettingsSnapshot settings,
        bool hasPickupProof,
        bool hasDeliveryProof,
        bool hasPreTripDvir,
        bool hasPostTripDvir,
        string? preTripDvirResult,
        string? postTripDvirResult,
        TripCaptureAttachmentState attachmentState)
    {
        var items = new List<TripCaptureReadinessItemResponse>
        {
            BuildItem(
                PreTripDvirKey,
                "Pre-trip DVIR submitted",
                hasPreTripDvir,
                settings.RequirePreTripDvirBeforeStart,
                hasPreTripDvir ? null : "Submit pre-trip DVIR before starting the trip."),
            BuildItem(
                PostTripDvirKey,
                "Post-trip DVIR submitted",
                hasPostTripDvir,
                settings.RequirePostTripDvirBeforeComplete,
                hasPostTripDvir ? null : "Submit post-trip DVIR before completing the trip."),
            BuildItem(
                PickupProofKey,
                "Pickup proof captured",
                hasPickupProof,
                settings.RequirePickupProofBeforeStart,
                hasPickupProof ? null : "Capture pickup proof before starting the trip."),
            BuildItem(
                DeliveryProofKey,
                "Delivery proof captured",
                hasDeliveryProof,
                settings.RequireDeliveryProofBeforeComplete,
                hasDeliveryProof ? null : "Capture delivery proof before completing the trip."),
            BuildItem(
                PreTripDvirPassKey,
                "Pre-trip DVIR pass or conditional",
                !hasPreTripDvir
                    || string.Equals(preTripDvirResult, DvirInspectionResults.Pass, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(preTripDvirResult, DvirInspectionResults.Conditional, StringComparison.OrdinalIgnoreCase),
                settings.BlockTripStartOnDvirFail && hasPreTripDvir,
                settings.BlockTripStartOnDvirFail && hasPreTripDvir
                && string.Equals(preTripDvirResult, DvirInspectionResults.Fail, StringComparison.OrdinalIgnoreCase)
                    ? "Pre-trip DVIR failed — resolve defects or update inspection before starting."
                    : null),
            BuildItem(
                PostTripDvirPassKey,
                "Post-trip DVIR pass or conditional",
                !hasPostTripDvir
                    || string.Equals(postTripDvirResult, DvirInspectionResults.Pass, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(postTripDvirResult, DvirInspectionResults.Conditional, StringComparison.OrdinalIgnoreCase),
                settings.BlockTripCompleteOnDvirFail && hasPostTripDvir,
                settings.BlockTripCompleteOnDvirFail && hasPostTripDvir
                && string.Equals(postTripDvirResult, DvirInspectionResults.Fail, StringComparison.OrdinalIgnoreCase)
                    ? "Post-trip DVIR failed — resolve defects before completing the trip."
                    : null),
            BuildItem(
                PickupProofPhotoKey,
                "Pickup proof photo attached",
                attachmentState.HasPickupProofPhoto,
                settings.RequirePickupProofPhotoBeforeStart && hasPickupProof,
                attachmentState.HasPickupProofPhoto ? null : "Attach a pickup proof photo before starting the trip."),
            BuildItem(
                PreTripDvirPhotoKey,
                "Pre-trip DVIR photo attached",
                attachmentState.HasPreTripDvirPhoto,
                settings.RequirePreTripDvirPhotoBeforeStart && hasPreTripDvir,
                attachmentState.HasPreTripDvirPhoto ? null : "Attach a pre-trip DVIR photo before starting the trip."),
            BuildItem(
                DeliveryProofPhotoKey,
                "Delivery proof photo attached",
                attachmentState.HasDeliveryProofPhoto,
                settings.RequireDeliveryProofPhotoBeforeComplete && hasDeliveryProof,
                attachmentState.HasDeliveryProofPhoto ? null : "Attach a delivery proof photo before completing the trip."),
            BuildItem(
                DeliverySignatureKey,
                "Delivery signature captured",
                attachmentState.HasDeliverySignature,
                settings.RequireDeliverySignatureBeforeComplete && hasDeliveryProof,
                attachmentState.HasDeliverySignature ? null : "Capture a delivery signature before completing the trip."),
            BuildItem(
                PostTripDvirPhotoKey,
                "Post-trip DVIR photo attached",
                attachmentState.HasPostTripDvirPhoto,
                settings.RequirePostTripDvirPhotoBeforeComplete && hasPostTripDvir,
                attachmentState.HasPostTripDvirPhoto ? null : "Attach a post-trip DVIR photo before completing the trip."),
        };

        var startReady = items
            .Where(x => x.Key is PreTripDvirKey or PickupProofKey or PreTripDvirPassKey or PickupProofPhotoKey or PreTripDvirPhotoKey)
            .Where(x => x.Required)
            .All(x => x.Satisfied);

        var completeReady = items
            .Where(x => x.Key is PostTripDvirKey or DeliveryProofKey or PostTripDvirPassKey or DeliveryProofPhotoKey or DeliverySignatureKey or PostTripDvirPhotoKey)
            .Where(x => x.Required)
            .All(x => x.Satisfied);

        return new TripCaptureReadinessResponse(
            tripId,
            dispatchStatus,
            startReady,
            completeReady,
            items);
    }

    public static void EnsureCanStart(TripCaptureReadinessResponse readiness)
    {
        if (readiness.CanStartTrip)
        {
            return;
        }

        var blocker = readiness.Items.FirstOrDefault(x =>
            x.Required
            && !x.Satisfied
            && x.Key is PreTripDvirKey or PickupProofKey or PreTripDvirPassKey or PickupProofPhotoKey or PreTripDvirPhotoKey);

        throw new StlApiException(
            "driver_portal.capture_not_ready",
            blocker?.Message ?? "Trip capture requirements are not satisfied for start.",
            409);
    }

    public static void EnsureCanComplete(TripCaptureReadinessResponse readiness)
    {
        if (readiness.CanCompleteTrip)
        {
            return;
        }

        var blocker = readiness.Items.FirstOrDefault(x =>
            x.Required
            && !x.Satisfied
            && x.Key is PostTripDvirKey or DeliveryProofKey or PostTripDvirPassKey or DeliveryProofPhotoKey or DeliverySignatureKey or PostTripDvirPhotoKey);

        throw new StlApiException(
            "driver_portal.capture_not_ready",
            blocker?.Message ?? "Trip capture requirements are not satisfied for complete.",
            409);
    }

    private static TripCaptureReadinessItemResponse BuildItem(
        string key,
        string label,
        bool satisfied,
        bool required,
        string? message) =>
        new(key, label, satisfied, required, message);
}

public sealed record TripExecutionSettingsSnapshot(
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
    bool RequirePostTripDvirPhotoBeforeComplete)
{
    public static TripExecutionSettingsSnapshot Defaults { get; } = new(
        RequirePreTripDvirBeforeStart: true,
        RequirePostTripDvirBeforeComplete: false,
        RequireDeliveryProofBeforeComplete: false,
        RequirePickupProofBeforeStart: false,
        BlockTripStartOnDvirFail: true,
        BlockTripCompleteOnDvirFail: true,
        RequirePickupProofPhotoBeforeStart: false,
        RequireDeliveryProofPhotoBeforeComplete: false,
        RequireDeliverySignatureBeforeComplete: false,
        RequirePreTripDvirPhotoBeforeStart: false,
        RequirePostTripDvirPhotoBeforeComplete: false);
}
