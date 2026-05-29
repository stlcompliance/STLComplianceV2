using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class TripExecutionCaptureRulesTests
{
    [Fact]
    public void ValidateDvirSubmit_requires_defect_notes_on_fail()
    {
        var ex = Assert.Throws<STLCompliance.Shared.Contracts.StlApiException>(() =>
            TripExecutionCaptureRules.ValidateDvirSubmit("fail", "  "));

        Assert.Equal("trip_dvir.defect_notes_required", ex.Code);
    }

    [Fact]
    public void BuildReadiness_blocks_start_without_pre_trip_when_required()
    {
        var settings = TripExecutionSettingsSnapshot.Defaults;
        var readiness = TripExecutionCaptureRules.BuildReadiness(
            Guid.NewGuid(),
            TripDispatchStatuses.Dispatched,
            settings,
            hasPickupProof: false,
            hasDeliveryProof: false,
            hasPreTripDvir: false,
            hasPostTripDvir: false,
            preTripDvirResult: null,
            postTripDvirResult: null,
            TripCaptureAttachmentState.Empty);

        Assert.False(readiness.CanStartTrip);
        Assert.Contains(readiness.Items, x => x.Key == TripExecutionCaptureRules.PreTripDvirKey && !x.Satisfied);
    }

    [Fact]
    public void BuildReadiness_blocks_start_on_pre_trip_fail_when_enabled()
    {
        var settings = TripExecutionSettingsSnapshot.Defaults;
        var readiness = TripExecutionCaptureRules.BuildReadiness(
            Guid.NewGuid(),
            TripDispatchStatuses.Dispatched,
            settings,
            hasPickupProof: false,
            hasDeliveryProof: false,
            hasPreTripDvir: true,
            hasPostTripDvir: false,
            preTripDvirResult: DvirInspectionResults.Fail,
            postTripDvirResult: null,
            TripCaptureAttachmentState.Empty);

        Assert.False(readiness.CanStartTrip);
    }

    [Fact]
    public void BuildReadiness_allows_start_after_pre_trip_pass()
    {
        var settings = TripExecutionSettingsSnapshot.Defaults;
        var readiness = TripExecutionCaptureRules.BuildReadiness(
            Guid.NewGuid(),
            TripDispatchStatuses.Dispatched,
            settings,
            hasPickupProof: false,
            hasDeliveryProof: false,
            hasPreTripDvir: true,
            hasPostTripDvir: false,
            preTripDvirResult: DvirInspectionResults.Pass,
            postTripDvirResult: null,
            TripCaptureAttachmentState.Empty);

        Assert.True(readiness.CanStartTrip);
    }

    [Fact]
    public void BuildReadiness_blocks_start_without_pickup_photo_when_required()
    {
        var settings = TripExecutionSettingsSnapshot.Defaults with
        {
            RequirePreTripDvirBeforeStart = false,
            RequirePickupProofBeforeStart = true,
            RequirePickupProofPhotoBeforeStart = true,
        };

        var readiness = TripExecutionCaptureRules.BuildReadiness(
            Guid.NewGuid(),
            TripDispatchStatuses.Dispatched,
            settings,
            hasPickupProof: true,
            hasDeliveryProof: false,
            hasPreTripDvir: false,
            hasPostTripDvir: false,
            preTripDvirResult: null,
            postTripDvirResult: null,
            new TripCaptureAttachmentState(
                HasPickupProofPhoto: false,
                HasDeliveryProofPhoto: false,
                HasDeliverySignature: false,
                HasPreTripDvirPhoto: false,
                HasPostTripDvirPhoto: false));

        Assert.False(readiness.CanStartTrip);
        Assert.Contains(readiness.Items, x => x.Key == TripExecutionCaptureRules.PickupProofPhotoKey);
    }
}
