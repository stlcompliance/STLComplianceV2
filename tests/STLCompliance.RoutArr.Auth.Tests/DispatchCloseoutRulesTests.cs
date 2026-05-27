using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchCloseoutRulesTests
{
    [Fact]
    public void PlanTrip_complete_blocks_planned_without_driver()
    {
        var trip = new Trip
        {
            DispatchStatus = TripDispatchStatuses.Planned,
            AssignedDriverPersonId = null,
        };

        var plan = DispatchCloseoutRules.PlanTrip(trip, DispatchCloseoutRules.TripDispositionComplete);

        Assert.False(plan.CanApply);
        Assert.Equal("trip.closeout_complete_blocked", plan.BlockCode);
    }

    [Fact]
    public void PlanTrip_cancel_allows_planned_trip()
    {
        var trip = new Trip
        {
            DispatchStatus = TripDispatchStatuses.Planned,
        };

        var plan = DispatchCloseoutRules.PlanTrip(trip, DispatchCloseoutRules.TripDispositionCancel);

        Assert.True(plan.CanApply);
        Assert.Equal(TripDispatchStatuses.Cancelled, plan.TargetStatus);
        Assert.Single(plan.TransitionSteps);
    }

    [Fact]
    public void PlanTrip_complete_chains_dispatched_to_completed()
    {
        var trip = new Trip
        {
            DispatchStatus = TripDispatchStatuses.Dispatched,
            AssignedDriverPersonId = "driver-1",
        };

        var plan = DispatchCloseoutRules.PlanTrip(trip, DispatchCloseoutRules.TripDispositionComplete);

        Assert.True(plan.CanApply);
        Assert.Equal(
            [TripDispatchStatuses.InProgress, TripDispatchStatuses.Completed],
            plan.TransitionSteps);
    }

    [Fact]
    public void PlanStop_skip_moves_pending_to_skipped()
    {
        var stop = new RouteStop { StopStatus = RouteStopStatuses.Pending };

        var plan = DispatchCloseoutRules.PlanStop(stop, DispatchCloseoutRules.StopDispositionSkip);

        Assert.True(plan.CanApply);
        Assert.Equal(RouteStopStatuses.Skipped, plan.TargetStopStatus);
    }

    [Fact]
    public void PlanStop_complete_blocks_pending_stop()
    {
        var stop = new RouteStop { StopStatus = RouteStopStatuses.Pending };

        var plan = DispatchCloseoutRules.PlanStop(stop, DispatchCloseoutRules.StopDispositionComplete);

        Assert.False(plan.CanApply);
        Assert.Equal("route_stop.arrival_required", plan.BlockCode);
    }

    [Fact]
    public void PlanRoute_complete_requires_terminal_stops()
    {
        var route = new DispatchRoute { RouteStatus = RouteStatuses.Active };

        var blocked = DispatchCloseoutRules.PlanRoute(
            route,
            DispatchCloseoutRules.TripDispositionComplete,
            allStopsTerminal: false);

        Assert.False(blocked.CanApply);
        Assert.Equal("route.stops_open", blocked.BlockCode);

        var allowed = DispatchCloseoutRules.PlanRoute(
            route,
            DispatchCloseoutRules.TripDispositionComplete,
            allStopsTerminal: true);

        Assert.True(allowed.CanApply);
        Assert.Equal(RouteStatuses.Completed, allowed.TargetRouteStatus);
    }
}
