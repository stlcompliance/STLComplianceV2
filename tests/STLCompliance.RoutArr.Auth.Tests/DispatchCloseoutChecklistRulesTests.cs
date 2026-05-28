using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchCloseoutChecklistRulesTests
{
    [Fact]
    public void BuildTripChecklist_cancel_marks_planned_trip_ready()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            TripNumber = "TR-CHK-1",
            DispatchStatus = TripDispatchStatuses.Planned,
        };
        var plan = DispatchCloseoutRules.PlanTrip(trip, DispatchCloseoutRules.TripDispositionCancel);

        var checklist = DispatchCloseoutChecklistRules.BuildTripChecklist(
            trip,
            DispatchCloseoutRules.TripDispositionCancel,
            openStopCount: 0,
            openRouteCount: 0,
            openExceptionCount: 0,
            hasProof: false,
            hasPreTripDvir: false,
            hasPostTripDvir: false,
            plan);

        Assert.True(checklist.ReadyForCloseout);
        Assert.Contains(checklist.Items, x => x.Key == DispatchCloseoutChecklistRules.TripDispositionReadyKey && x.Satisfied);
    }

    [Fact]
    public void BuildTripChecklist_complete_blocks_open_stops()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            TripNumber = "TR-CHK-2",
            DispatchStatus = TripDispatchStatuses.InProgress,
            AssignedDriverPersonId = "driver-1",
        };
        var plan = DispatchCloseoutRules.PlanTrip(trip, DispatchCloseoutRules.TripDispositionComplete);

        var checklist = DispatchCloseoutChecklistRules.BuildTripChecklist(
            trip,
            DispatchCloseoutRules.TripDispositionComplete,
            openStopCount: 2,
            openRouteCount: 1,
            openExceptionCount: 0,
            hasProof: true,
            hasPreTripDvir: true,
            hasPostTripDvir: false,
            plan);

        Assert.False(checklist.ReadyForCloseout);
        Assert.Contains(checklist.Items, x => x.Key == DispatchCloseoutChecklistRules.StopsClosedKey && !x.Satisfied);
    }
}
